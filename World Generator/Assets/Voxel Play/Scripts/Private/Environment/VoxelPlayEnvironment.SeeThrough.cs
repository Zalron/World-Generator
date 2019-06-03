using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VoxelPlay {

	public partial class VoxelPlayEnvironment : MonoBehaviour {

		VoxelIndex[] occludedIndices;
		VoxelChunk[] occludedChunks;
		int occludedIndicesCount = 0;


		public void ManageSeeThrough () {

			VoxelSetHidden (occludedIndices, occludedIndicesCount, false);
			Camera cam = currentCamera;
			if (cam == null)
				return;
			Vector3 camPos = cam.transform.position;

			if (seeThroughTarget == null) {
				if (characterController != null) {
					seeThroughTarget = characterController.gameObject;
				}
				if (seeThroughTarget == null)
					return;
			}
			Vector3 targetPos = seeThroughTarget.transform.position;

			if (occludedIndices == null || occludedIndices.Length == 0) {
				occludedIndices = new VoxelIndex[256];
			}
			if (occludedChunks == null || occludedChunks.Length == 0) {
				occludedChunks = new VoxelChunk[20];
			}

			int chunkCount = LineCast (targetPos, camPos, occludedChunks);

			// Add surrounding chunks
			int flag = Time.frameCount;
			for (int k = 0; k < chunkCount; k++) {
				VoxelChunk chunk = occludedChunks [k];
				chunk.tempFlag = flag;
			}

			int lineChunks = chunkCount;
			for (int k = 0; k < lineChunks; k++) {
				VoxelChunk chunk = occludedChunks [k];
				VoxelChunk n = chunk.top; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
				n = chunk.bottom; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
				n = chunk.left; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
				n = chunk.right; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
				n = chunk.forward; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
				n = chunk.back; 
				if (n != null && n.tempFlag != flag) {
					if (chunkCount >= occludedChunks.Length) {
						occludedChunks = occludedChunks.Extend ();
					}
					occludedChunks [chunkCount++] = n;
					n.tempFlag = flag;
				}
			}

			float distToTarget = Vector3.Distance (targetPos, camPos);
			Vector3 cylinderAxis = (camPos - targetPos) / distToTarget;
			float radiusSqr = seeThroughRadius * seeThroughRadius;
			float minY = targetPos.y + seeThroughHeightOffset;
			occludedIndicesCount = 0;
			Vector3 voxelPosition = Misc.vector3zero;
			for (int k = 0; k < chunkCount; k++) {
				VoxelChunk chunk = occludedChunks [k];
				Vector3 chunkPosBase = chunk.position;
				chunkPosBase.x = chunkPosBase.x - 8 + 0.5f;
				chunkPosBase.y = chunkPosBase.y - 8 + 0.5f;
				chunkPosBase.z = chunkPosBase.z - 8 + 0.5f;
				for (int voxelIndex = 0; voxelIndex < chunk.voxels.Length; voxelIndex++) {
					if (chunk.voxels [voxelIndex].hasContent == 1) {
						int py = voxelIndex >> 8;
						voxelPosition.y = py + chunkPosBase.y;
						if (voxelPosition.y < minY)
							continue;
						int pz = (voxelIndex & 0xF0) >> 4;
						int px = voxelIndex & 0xF;
						voxelPosition.x = px + chunkPosBase.x;
						voxelPosition.z = pz + chunkPosBase.z;

						// Check if it's inside the cylinder
						Vector3 v = voxelPosition;
						v.x -= targetPos.x;
						v.y -= targetPos.y;
						v.z -= targetPos.z;
						float cylinderDist = v.x * cylinderAxis.x + v.y * cylinderAxis.y + v.z * cylinderAxis.z; // Vector3.Dot (v, cylinderAxis);
						if (cylinderDist < 0 || cylinderDist > distToTarget)
							continue;

						v.x -= cylinderDist * cylinderAxis.x;
						v.y -= cylinderDist * cylinderAxis.y;
						v.z -= cylinderDist * cylinderAxis.z;
						float orthDistanceSqr = v.x * v.x + v.y * v.y + v.z * v.z; // (v - cylinderDist * cylinderAxis).sqrMagnitude;

						if (orthDistanceSqr < radiusSqr) {
							if (occludedIndicesCount >= occludedIndices.Length) {
								occludedIndices = occludedIndices.Extend ();
							}
							occludedIndices [occludedIndicesCount].chunk = chunk;
							occludedIndices [occludedIndicesCount].voxelIndex = voxelIndex;
							occludedIndicesCount++;
						}
					}
				}
			}
			VoxelSetHidden (occludedIndices, occludedIndicesCount, true);
		}

					
	}



}
