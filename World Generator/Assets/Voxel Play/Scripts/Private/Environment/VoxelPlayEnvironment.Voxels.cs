using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelPlay {

	public partial class VoxelPlayEnvironment : MonoBehaviour {

		public enum HiddenFilter {
			OnlySeeThroughAllowedVoxels = 0,
			AnyVoxel = 1
		}


		const byte FULL_OPAQUE = 15;

		[NonSerialized]
		public VoxelDefinition[] voxelDefinitions;
		public int voxelDefinitionsCount;

		GameObject defaultVoxelPrefab;
		List<VoxelChunk> voxelPlaceFastAffectedChunks;
		GameObject voxelHighlightGO;


		float[] collapsingOffsets = new float[] {
			0, 1, 0,
			-1, 1, -1,
			1, 1, 1,
			-1, 1, 1,
			1, 1, -1,
			1, 1, 0,
			0, 1, 1,
			-1, 1, 0,
			0, 1, -1,
			-1, 1, -1,
			1, 0, 1,
			-1, 0, 1,
			1, 0, -1,
			1, 0, 0,
			0, 0, 1,
			-1, 0, 0,
			0, 0, -1,
			-1, 0, -1
		};

		void VoxelDestroyFast (VoxelChunk chunk, int voxelIndex) {

			// Ensure there's content on this position
			if (chunk.voxels [voxelIndex].hasContent != 1)
				return;

			if (OnVoxelBeforeDestroyed != null) {
				OnVoxelBeforeDestroyed (chunk, voxelIndex);
			}

			// 1) Remove placeholder if exists
			VoxelPlaceholderDestroy (chunk, voxelIndex);

			// 2) Clears voxel
			bool triggerCollapse = voxelDefinitions [chunk.voxels [voxelIndex].typeIndex].triggerCollapse;
			VoxelDestroyFastSingle (chunk, voxelIndex);

			// Update lightmap and renderers
//			UpdateChunkRR (chunk); // removed to avoid temporary black voxel until lightmap is computed.
			ChunkRequestRefresh (chunk, true, true);

			// Force rebuild neighbour meshes if destroyed voxel is on a border
			RebuildNeighbours (chunk, voxelIndex);

			if (OnChunkChanged != null) {
				OnChunkChanged (chunk);
			}
			if (OnVoxelDestroyed != null) {
				OnVoxelDestroyed (chunk, voxelIndex);
			}

			// Check if it was surrounded by water. If it was, add water expander
			Vector3 voxelPosition = GetVoxelPosition (chunk, voxelIndex);
			MakeSurroundingWaterExpand (chunk, voxelIndex, voxelPosition);

			// Check if voxels on top can fall
			if (triggerCollapse && world.collapseOnDestroy) {
				VoxelCollapse (voxelPosition, world.collapseAmount, null, world.consolidateDelay); 
			}
		}

		void VoxelDestroyFastSingle (VoxelChunk chunk, int voxelIndex) {
			chunk.voxels [voxelIndex].Clear (effectiveGlobalIllumination ? (byte)0 : (byte)15);
			chunk.modified = true;
		}


		/// <summary>
		/// Puts a voxel in the given position. Takes care of informing neighbour chunks.
		/// </summary>
		/// <returns>Returns the affected chunk and voxel index</returns>
		/// <param name="position">Position.</param>
		/// <param name="voxel">Voxel.</param>
		void VoxelPlaceFast (Vector3 position, VoxelDefinition voxelType, out VoxelChunk chunk, out int voxelIndex, Color32 tintColor, float amount = 1f, int rotation = 0) {

			VoxelSingleSet (position, voxelType, out chunk, out voxelIndex, tintColor);

			// Apply rotation
			if (voxelType.allowsTextureRotation) {
				chunk.voxels [voxelIndex].SetTextureRotation (rotation);
			}

			// If it's water, add flood
			if (voxelType.spreads) {
				chunk.voxels [voxelIndex].SetWaterLevel ((Mathf.CeilToInt (amount * 15f)));
				AddWaterFlood (ref position, voxelType);
			}

			chunk.modified = true;

			// Update neighbours
			UpdateChunkRR (chunk);

			// Triggers event
			if (OnChunkChanged != null) {
				OnChunkChanged (chunk);
			}
		}


		/// <summary>
		/// Internal method that puts a voxel in a given position. This method does not inform to neighbours. Only used by non-contiguous structures, like trees or vegetation.
		/// For terrain or large scale buildings, use VoxelPlaceFast.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="voxelType">Voxel type.</param>
		/// <param name="chunk">Chunk.</param>
		/// <param name="voxelIndex">Voxel index.</param>
		void VoxelSingleSet (Vector3 position, VoxelDefinition voxelType, out VoxelChunk chunk, out int voxelIndex) {
			VoxelSingleSet (position, voxelType, out chunk, out voxelIndex, Misc.color32White);
		}

		/// <summary>
		/// Internal method that puts a voxel in a given position. This method does not inform to neighbours. Only used by non-contiguous structures, like trees or vegetation.
		/// For terrain or large scale buildings, use VoxelPlaceFast.
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="voxelType">Voxel type.</param>
		/// <param name="chunk">Chunk.</param>
		/// <param name="voxelIndex">Voxel index.</param>
		void VoxelSingleSet (Vector3 position, VoxelDefinition voxelType, out VoxelChunk chunk, out int voxelIndex, Color32 tintColor) {
			if (GetVoxelIndex (position, out chunk, out voxelIndex)) {
				if (OnVoxelBeforePlace != null) {
					OnVoxelBeforePlace (position, chunk, voxelIndex, ref voxelType, ref tintColor);
					if (voxelType == null)
						return;
				}
				chunk.voxels [voxelIndex].Set (voxelType, tintColor);
			}
		}

		/// <summary>
		/// Internal method that clears any voxel in a given position. This method does not inform to neighbours.
		/// </summary>
		/// <param name="position">Position.</param>
		void VoxelSingleClear (Vector3 position, out VoxelChunk chunk, out int voxelIndex) {
			if (GetVoxelIndex (position, out chunk, out voxelIndex)) {
				VoxelDestroyFastSingle (chunk, voxelIndex);
			}
		}

		/// <summary>
		/// Converts a voxel into dynamic type
		/// </summary>
		/// <param name="chunk">Chunk.</param>
		/// <param name="voxelIndex">Voxel index.</param>
		GameObject VoxelSetDynamic (VoxelChunk chunk, int voxelIndex, bool addRigidbody, float duration) {
			if (chunk == null || chunk.voxels [voxelIndex].hasContent == 0)
				return null;

			VoxelDefinition vd = voxelDefinitions [chunk.voxels [voxelIndex].typeIndex];
			if (!vd.renderType.supportsDynamic ()) {
				return null;
			}

			VoxelPlaceholder placeholder = GetVoxelPlaceholder (chunk, voxelIndex, true);
			if (placeholder == null)
				return null;

			// Add rigid body
			if (addRigidbody) {
				Rigidbody rb = placeholder.GetComponent<Rigidbody> ();
				if (rb == null) {
					placeholder.rb = placeholder.gameObject.AddComponent<Rigidbody> ();
				}
			}

			// If it's a custom model ignore it as it's already a gameobject
			if (placeholder.modelMeshFilter != null)
				return placeholder.gameObject;

			VoxelDefinition vdDyn = vd.dynamicDefinition;

			if (vdDyn == null) {
				// Setup and save voxel definition
				vd.dynamicDefinition = vdDyn = ScriptableObject.CreateInstance<VoxelDefinition> ();
				vdDyn.isDynamic = true;
				vdDyn.doNotSave = true;
				vdDyn.staticDefinition = vd;
				vdDyn.renderType = RenderType.Custom;
				vdDyn.textureIndexBottom = vd.textureIndexBottom;
				vdDyn.textureIndexSide = vd.textureIndexSide;
				vdDyn.textureIndexTop = vd.textureIndexTop;
				vdDyn.textureThumbnailTop = vd.textureThumbnailTop;
				vdDyn.textureThumbnailSide = vd.textureThumbnailSide;
				vdDyn.textureThumbnailBottom = vd.textureThumbnailBottom;
				vdDyn.scale = vd.scale;
				vdDyn.offset = vd.offset;
				vdDyn.offsetRandomRange = vd.offsetRandomRange;
				vdDyn.rotation = vd.rotation;
				vdDyn.rotationRandomY = vd.rotationRandomY;
				vdDyn.sampleColor = vd.sampleColor;
				vdDyn.promotesTo = vd.promotesTo;
				vdDyn.playerDamageDelay = vd.playerDamageDelay;
				vdDyn.playerDamage = vd.playerDamage;
				vdDyn.pickupSound = vd.pickupSound;
				vdDyn.landingSound = vd.landingSound;
				vdDyn.jumpSound = vd.jumpSound;
				vdDyn.impactSound = vd.impactSound;
				vdDyn.footfalls = vd.footfalls;
				vdDyn.destructionSound = vd.destructionSound;
				vdDyn.canBeCollected = vd.canBeCollected;
				vdDyn.dropItem = GetItemDefinition (ItemCategory.Voxel, vd);
				vdDyn.buildSound = vd.buildSound;
				vdDyn.navigatable = true;
				vdDyn.windAnimation = false;
				vdDyn.model = MakeDynamicCubeFromVoxel (chunk, voxelIndex);
				vdDyn.name = vd.name + " (Dynamic)";
				AddVoxelTextures (vdDyn);
			}

			// Clear any vegetation on top if voxel can be moved (has a rigidbody) to avoid floating grass block
			if (placeholder.rb != null) {
				VoxelChunk topChunk;
				int topIndex;
				if (GetVoxelIndex (chunk, voxelIndex, 0, 1, 0, out topChunk, out topIndex)) {
					if (topChunk.voxels [topIndex].hasContent == 1 && voxelDefinitions [topChunk.voxels [topIndex].typeIndex].renderType == RenderType.CutoutCross) {
						VoxelDestroyFast (topChunk, topIndex);
					}
				}
			}
			Color32 color = chunk.voxels [voxelIndex].color;
			chunk.voxels [voxelIndex].Set (vdDyn, color);

			if (duration > 0) {
				placeholder.SetCancelDynamic (duration);
			}

			// Refresh neighbours
			RebuildNeighbours (chunk, voxelIndex);

			return placeholder.gameObject;
		}

		/// <summary>
		/// Finds all voxels with "willCollapse" connected to a given position
		/// </summary>
		/// <returns>The crumbly voxel indices.</returns>
		/// <param name="position">Position.</param>
		/// <param name="results">Results.</param>
		int GetCrumblyVoxelIndices (Vector3 position, int amount, List<VoxelIndex> voxelIndices = null) {
			if (voxelIndices == null) {
				voxelIndices = tempVoxelIndices;
			}
			voxelIndices.Clear ();
			tempVoxelPositions.Clear ();
			tempVoxelIndicesCount = 0;
			GetCrumblyVoxelRecursive (position + Misc.vector3one, position, amount, voxelIndices);
			return tempVoxelIndicesCount;
		}

		void GetCrumblyVoxelRecursive (Vector3 originalPosition, Vector3 position, int amount, List<VoxelIndex> voxelIndices) {
			if (tempVoxelIndicesCount >= amount)
				return;
			
			VoxelChunk chunk;
			int voxelIndex;
			int c = 0;
			bool dummy;
			for (int k = 0; k < collapsingOffsets.Length; k += 3) {
				Vector3 pos = position;
				pos.x += collapsingOffsets [k];
				pos.y += collapsingOffsets [k + 1];
				pos.z += collapsingOffsets [k + 2];
				float dx = pos.x > originalPosition.x ? pos.x - originalPosition.x : originalPosition.x - pos.x;
				float dz = pos.z > originalPosition.z ? pos.z - originalPosition.z : originalPosition.z - pos.z;
				if (dx > 8 || dz > 8)
					continue;
				if (!tempVoxelPositions.TryGetValue (pos, out dummy)) {
					tempVoxelPositions [pos] = true;
					if (GetVoxelIndex (pos, out chunk, out voxelIndex, false) && chunk.voxels [voxelIndex].hasContent == 1 && chunk.voxels [voxelIndex].opaque >= 3 && voxelDefinitions [chunk.voxels [voxelIndex].typeIndex].willCollapse) {
						VoxelIndex vi = new VoxelIndex ();
						vi.chunk = chunk;
						vi.voxelIndex = voxelIndex;
						vi.position = pos;
						voxelIndices.Add (vi);
						tempVoxelIndicesCount++;
						c++;
						if (tempVoxelIndicesCount >= amount)
							break;
					}
				}
			}
			int lastCount = tempVoxelIndicesCount;
			for (int k = 1; k <= c; k++) {
				GetCrumblyVoxelRecursive (originalPosition, tempVoxelIndices [lastCount - k].position, amount, voxelIndices);
			}
		}

		/// <summary>
		/// Returns the default voxel prefab (usually a cube; the prefab is located in Defaults folder)
		/// </summary>
		/// <returns>The default voxel prefab.</returns>
		GameObject GetDefaultVoxelPrefab () {
			if (defaultVoxelPrefab == null) {
				defaultVoxelPrefab = Resources.Load<GameObject> ("VoxelPlay/Defaults/DefaultModel/Cube");
			}
			return defaultVoxelPrefab;
		}

		/// <summary>
		/// Toggles on/off hidden voxels.
		/// </summary>
		void ToggleHiddenVoxels (VoxelChunk chunk, bool show) {

			if (chunk == null)
				return;
			
			FastHashSet<VoxelExtraData> data = chunk.voxelsExtraData;
			if (data == null)
				return;

			int count = data.Count;
			Voxel[] voxels = chunk.voxels;

			for (int k = 0; k < count; k++) {
				int voxelIndex = data.entries [k].key;
				if (voxelIndex >= 0) {
					if (data.entries [k].value.hidden) {
						if (show) {
							voxels [voxelIndex].typeIndex = data.entries [k].value.hiddenTypeIndex;
							voxels [voxelIndex].opaque = data.entries [k].value.hiddenOpaque;
							voxels [voxelIndex].light = data.entries [k].value.hiddenLight;
						} else {
							ushort typeIndex = voxels [voxelIndex].typeIndex;
							data.entries [k].value.hiddenTypeIndex = typeIndex;
							data.entries [k].value.hiddenOpaque = voxels [voxelIndex].opaque;
							data.entries [k].value.hiddenLight = voxels [voxelIndex].light;
							VoxelDefinition vd = voxelDefinitions [typeIndex];
							switch (vd.seeThroughMode) {
							case SeeThroughMode.None:
								voxels [voxelIndex].typeIndex = 0; // null voxel whose renderType = Empty (but genereate colliders)
								voxels [voxelIndex].opaque = 0;
								break;
							case SeeThroughMode.ReplaceVoxel:
								voxels [voxelIndex].typeIndex = vd.seeThroughVoxel.index; // null voxel whose renderType = Empty (but genereate colliders)
								voxels [voxelIndex].opaque = vd.seeThroughVoxel.opaque;
								break;
							case SeeThroughMode.Transparency:
								voxels [voxelIndex].typeIndex = (ushort)vd.seeThroughVoxelTempTransp; // null voxel whose renderType = Empty (but genereate colliders)
								voxels [voxelIndex].opaque = 0;
								break;

							}
							voxels [voxelIndex].light = 15;
						}
					}
				}
			}
		}

		void VoxelSetHiddenOne(VoxelChunk chunk, int voxelIndex, bool hidden, HiddenFilter hiddenFilter) {
			
			bool suitable = chunk.voxels [voxelIndex].hasContent == 1 && (hiddenFilter == HiddenFilter.AnyVoxel || voxelDefinitions [chunk.voxels [voxelIndex].typeIndex].seeThroughMode != SeeThroughMode.None);
			if (!suitable)
				return;

			if (chunk.voxelsExtraData == null) {
				if (!hidden)
					return;
				chunk.voxelsExtraData = new FastHashSet<VoxelExtraData> ();
			}
			VoxelExtraData hiddenVoxel = new VoxelExtraData ();
			hiddenVoxel.hidden = hidden;
			chunk.voxelsExtraData.Add (voxelIndex, hiddenVoxel, true);
		}

	}



}
