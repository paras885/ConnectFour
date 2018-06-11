using System;
using UnityEngine;

namespace Utils {
	public class CameraUtils {
		private GameState currentGameState;
		public CameraUtils (GameState gameState) {
			currentGameState = gameState;
		}

		public void setCamera(Camera camera) {
			camera.transform.position = new Vector3(
				(currentGameState.getColSize() - 1) / 2.0f,
				(currentGameState.getRowSize() - 1) / 2.0f,
				camera.transform.position.z
			);
		}
	}
}

