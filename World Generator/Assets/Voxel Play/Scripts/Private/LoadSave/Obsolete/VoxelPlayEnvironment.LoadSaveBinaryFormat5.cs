﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
using System.Text;
using System.Globalization;

namespace VoxelPlay {

	public partial class VoxelPlayEnvironment : MonoBehaviour {

		void LoadGameBinaryFileFormat_5 (BinaryReader br, bool preservePlayerPosition = false) {
			// Character controller transform position & rotation
			Vector3 pos = DecodeVector3Binary (br);
			Vector3 angles = DecodeVector3Binary (br);
			if (characterController != null) {
				if (!preservePlayerPosition) {
					characterController.transform.position = pos;
					characterController.transform.rotation = Quaternion.Euler (angles);
				}
			}
			// Character controller's camera local rotation
			angles = DecodeVector3Binary (br);
			if (!preservePlayerPosition) {
				cameraMain.transform.localRotation = Quaternion.Euler (angles);
				// Pass initial rotation to mouseLook script
				if (characterController != null) {
					characterController.GetComponent<VoxelPlayFirstPersonController> ().mouseLook.Init (characterController.transform, cameraMain.transform, null);
				}
			}

			// Read voxel definition table
			InitSaveGameStructs ();
			int vdCount = br.ReadInt16 (); 
			for (int k = 0; k < vdCount; k++) {
				saveVoxelDefinitionsList.Add (br.ReadString ());
			}

			int numChunks = br.ReadInt32 ();
			VoxelDefinition voxelDefinition = defaultVoxel;
			int prevVdIndex = -1;
			Color32 voxelColor = Misc.color32White;
			for (int c = 0; c < numChunks; c++) {
				// Read chunks
				// Get chunk position
				Vector3 chunkPosition = DecodeVector3Binary (br);
				VoxelChunk chunk = GetChunkUnpopulated (chunkPosition);
				byte isAboveSurface = br.ReadByte ();
				chunk.isAboveSurface = isAboveSurface == 1;
				chunk.back = chunk.bottom = chunk.left = chunk.right = chunk.forward = chunk.top = null;
				chunk.allowTrees = false;
				chunk.modified = true;
				chunk.isPopulated = true;
				chunk.voxelSignature = chunk.lightmapSignature = -1;
				chunk.renderState = ChunkRenderState.Pending;
				SetChunkOctreeIsDirty (chunkPosition, false);
				ChunkClearFast (chunk);
				// Read voxels
				int numWords = br.ReadInt16 ();
				for (int k = 0; k < numWords; k++) {
					// Voxel definition
					int vdIndex = br.ReadInt16 ();
					if (prevVdIndex != vdIndex) {
						if (vdIndex >= 0 && vdIndex < vdCount) {
							voxelDefinition = GetVoxelDefinition (saveVoxelDefinitionsList [vdIndex]);
							prevVdIndex = vdIndex;
						}
					}
					// RGB
					voxelColor.r = br.ReadByte ();
					voxelColor.g = br.ReadByte ();
					voxelColor.b = br.ReadByte ();
					// Voxel index
					int voxelIndex = br.ReadInt16 ();
					// Repetitions
					int repetitions = br.ReadInt16 ();

					if (voxelDefinition == null) {
						continue;
					}

					// Custom voxel flags
					byte flags = 0;
					if (voxelDefinition.renderType == RenderType.Water || voxelDefinition.renderType.supportsTextureRotation()) {
						flags = br.ReadByte ();
					} else if (voxelDefinition.renderType == RenderType.Custom) {
						byte hasCustomRotation = br.ReadByte ();
						if (hasCustomRotation == 1) {
							Vector3 voxelAngles = DecodeVector3Binary (br);
							saveVoxelCustomRotations.Add (GetVoxelPosition (chunkPosition, voxelIndex), voxelAngles);
						}
					}
					for (int i = 0; i < repetitions; i++) {
						chunk.voxels [voxelIndex + i].Set (voxelDefinition, voxelColor);
						if (voxelDefinition.renderType == RenderType.Water || voxelDefinition.renderType.supportsTextureRotation()) {
							chunk.voxels [voxelIndex + i].SetFlags (flags);
						}
					}
				}
				// Read light sources
				int lightCount = br.ReadInt16 ();
				VoxelHitInfo hitInfo = new VoxelHitInfo ();
				for (int k = 0; k < lightCount; k++) {
					// Voxel index
					hitInfo.voxelIndex = br.ReadInt16 ();
					// Voxel center
					hitInfo.voxelCenter = GetVoxelPosition (chunkPosition, hitInfo.voxelIndex);
					// Normal
					hitInfo.normal = DecodeVector3Binary (br);
					hitInfo.chunk = chunk;
					TorchAttach (hitInfo);
				}
			}
		}


// Preserved and commented for reference purposes
//
//		void SaveGameBinaryFormat_5 (BinaryWriter bw) {
//
//			// Build a table with all voxel definitions used in modified chunks
//			InitSaveGameStructs ();
//			VoxelDefinition last = null;
//			int count = 0;
//			int numChunks = 0;
//			foreach (KeyValuePair<int, CachedChunk>kv in cachedChunks) {
//				VoxelChunk chunk = kv.Value.chunk;
//				if (chunk != null && chunk.modified) {
//					numChunks++;
//					for (int k = 0; k < chunk.voxels.Length; k++) {
//						VoxelDefinition vd = chunk.voxels [k].type;
//						if (vd == null || vd == last || vd.isDynamic)
//							continue;
//						last = vd;
//						if (!saveVoxelDefinitionsDict.ContainsKey (vd)) {
//							saveVoxelDefinitionsDict [vd] = count++;
//							saveVoxelDefinitionsList.Add (vd.name);
//						}
//					}
//				}
//			}
//
//			// Header
//			bw.Write (SAVE_FILE_CURRENT_FORMAT);
//			// Character controller transform position
//			EncodeVector3Binary (bw, characterController.transform.position);
//			// Character controller transform rotation
//			EncodeVector3Binary (bw, characterController.transform.rotation.eulerAngles);
//			// Character controller's camera local rotation
//			EncodeVector3Binary (bw, cameraMain.transform.localRotation.eulerAngles);
//			// Add voxel definitions table
//			int vdCount = saveVoxelDefinitionsList.Count;
//			bw.Write ((Int16)vdCount);
//			for (int k = 0; k < vdCount; k++) {
//				bw.Write (saveVoxelDefinitionsList [k]);
//			}
//			// Add modified chunks
//			bw.Write (numChunks);
//			foreach (KeyValuePair<int, CachedChunk>kv in cachedChunks) {
//				VoxelChunk chunk = kv.Value.chunk;
//				if (chunk != null && chunk.modified) {
//					WriteChunkData_5 (bw, chunk);
//				}
//			}
//		}
//
//		void WriteChunkData_5 (BinaryWriter bw, VoxelChunk chunk) {
//			// Chunk position
//			EncodeVector3Binary (bw, chunk.position);
//			bw.Write (chunk.isAboveSurface ? (byte)1 : (byte)0);
//
//			int voxelDefinitionIndex = 0;
//			VoxelDefinition prevVD = null;
//
//
//			// Count voxels words
//			int k = 0;
//			int numWords = 0;
//			while (k < chunk.voxels.Length) {
//				if (chunk.voxels [k].hasContent == 1) {
//					VoxelDefinition voxelDefinition = chunk.voxels [k].type;
//					if (voxelDefinition.isDynamic) {
//						k++;
//						continue;
//					}
//					if (voxelDefinition != prevVD) {
//						if (!saveVoxelDefinitionsDict.TryGetValue (voxelDefinition, out voxelDefinitionIndex)) {
//							k++;
//							continue;
//						}
//						prevVD = voxelDefinition;
//					}
//					Color32 tintColor = chunk.voxels [k].color;
//					int flags = 0;
//					if (voxelDefinition.renderType == RenderType.Water || voxelDefinition.renderType.supportsTextureRotation()) {
//						flags = chunk.voxels [k].GetFlags ();
//					}
//					k++;
//					while (k < chunk.voxels.Length && chunk.voxels [k].type == voxelDefinition && chunk.voxels [k].color.r == tintColor.r && chunk.voxels [k].color.g == tintColor.g && chunk.voxels [k].color.b == tintColor.b && voxelDefinition.renderType != RenderType.Custom && chunk.voxels [k].GetFlags () == flags) {
//						k++;
//					}
//					numWords++;
//				} else {
//					k++;
//				}
//			}
//			bw.Write ((Int16)numWords);
//
//			// Write voxels
//			k = 0;
//			while (k < chunk.voxels.Length) {
//				if (chunk.voxels [k].hasContent == 1) {
//					int voxelIndex = k;
//					VoxelDefinition voxelDefinition = chunk.voxels [k].type;
//					if (voxelDefinition.isDynamic) {
//						k++;
//						continue;
//					}
//					if (voxelDefinition != prevVD) {
//						if (!saveVoxelDefinitionsDict.TryGetValue (voxelDefinition, out voxelDefinitionIndex)) {
//							k++;
//							continue;
//						}
//						prevVD = voxelDefinition;
//					}
//					Color32 tintColor = chunk.voxels [k].color;
//					byte flags = 0;
//					if (voxelDefinition.renderType == RenderType.Water || voxelDefinition.renderType.supportsTextureRotation()) {
//						flags = chunk.voxels [k].GetFlags ();
//					}
//					int repetitions = 1;
//					k++;
//					while (k < chunk.voxels.Length && chunk.voxels [k].type == voxelDefinition && chunk.voxels [k].color.r == tintColor.r && chunk.voxels [k].color.g == tintColor.g && chunk.voxels [k].color.b == tintColor.b && voxelDefinition.renderType != RenderType.Custom && chunk.voxels [k].GetFlags () == flags) {
//						repetitions++;
//						k++;
//					}
//					bw.Write ((Int16)voxelDefinitionIndex);
//					bw.Write (tintColor.r);
//					bw.Write (tintColor.g);
//					bw.Write (tintColor.b);
//					bw.Write ((Int16)voxelIndex);
//					bw.Write ((Int16)repetitions);
//					if (voxelDefinition.renderType == RenderType.Water || voxelDefinition.renderType.supportsTextureRotation()) {
//						bw.Write (flags);
//					} else if (voxelDefinition.renderType == RenderType.Custom) {
//						// Check rotation
//						VoxelPlaceholder placeholder = GetVoxelPlaceholder (chunk, voxelIndex, false);
//						if (placeholder != null) {
//							Vector3 angles = placeholder.transform.eulerAngles;
//							if (angles.x != 0 || angles.y != 0 || angles.z != 0) {
//								bw.Write ((byte)1); // has rotation
//								EncodeVector3Binary (bw, angles);
//							} else {
//								bw.Write ((byte)0);
//							}
//						} else {
//							bw.Write ((byte)0);
//						}
//					}
//				} else {
//					k++;
//				}
//			}
//
//			// Write number of light sources
//			int lightCount = chunk.lightSources != null ? chunk.lightSources.Count : 0;
//			bw.Write ((Int16)lightCount);
//			if (lightCount > 0) {
//				for (k = 0; k < lightCount; k++) {
//					VoxelHitInfo hi = chunk.lightSources [k].hitInfo;
//					bw.Write ((Int16)hi.voxelIndex);
//					EncodeVector3Binary (bw, hi.normal);
//				}
//			}
//		}
//
	}



}
