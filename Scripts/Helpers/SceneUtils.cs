using System;
using UnityEngine.SceneManagement;

namespace Utils {
	public class SceneUtils {
		public SceneUtils () {
		}

		public static void restartGame() {
			Scene scene = SceneManager.GetActiveScene(); 
			SceneManager.LoadScene(scene.name);
		}
	}
}

