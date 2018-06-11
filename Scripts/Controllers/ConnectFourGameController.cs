using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Utils;

public class ConnectFourGameController : MonoBehaviour {

	private GameObject board;
	private GameObject dummyPiece;
	private GameObject gameEndText;
	private GameObject restartButton;

	private Dictionary<UIConstants.Components, GameObject> mapOfPieces;

	private ConnectFourGameLogic gameLogic;
	private GameState gameState;
	private CameraUtils cameraUtils;
	private bool isMouseButtonPressed;
	private bool isPieceDropping;

	void Awake() {

		// Map game Objects with enum constants.
		mapOfPieces = new Dictionary<UIConstants.Components, GameObject> ();
		mapOfPieces.Add (UIConstants.Components.bluePiece,
			Resources.Load (UIConstants.Components.bluePiece.ToString ()) as GameObject);
		mapOfPieces.Add (UIConstants.Components.redPiece,
			Resources.Load (UIConstants.Components.redPiece.ToString ()) as GameObject);

		// Intialize gameState
		gameState = new GameState();
		gameState.intializeBoard (6, 7); // Row, Col Size of Board.

		gameLogic = new ConnectFourGameLogic (gameState);
		cameraUtils = new CameraUtils (gameState);

		isMouseButtonPressed = false;
		isPieceDropping = false;

		gameEndText = GameObject.Find (UIConstants.Components.GameEndText.ToString ());
		gameEndText.SetActive (false);

		restartButton = GameObject.Find (UIConstants.Components.RestartButton.ToString ());
		restartButton.SetActive (false);
		restartButton.GetComponent<Button>().onClick.AddListener(() => {Utils.SceneUtils.restartGame();});
	}

	// Use this for initialization
	void Start () {
		createBoard ();
		cameraUtils.setCamera (Camera.main);
		setGameStatusText();
		setRestartButton ();
	}

	void setGameStatusText() {
		gameEndText.transform.position = new Vector3 (
			(gameState.getColSize() - 1) / 2.0f, ((gameState.getRowSize() - 1) / 2.0f),
			1
		);
	}

	void setRestartButton() {
		restartButton.transform.position = new Vector3 (gameState.getRowSize(), -1, 1);
	}

	void createBoard() {
		board = Instantiate (Resources.Load (UIConstants.Components.board.ToString ()) as GameObject,
			new Vector3 (0, 0, 10), Quaternion.identity) as GameObject;

		GameObject hole = Resources.Load (UIConstants.Components.hole.ToString ()) as GameObject;
		for (int x = 0; x < gameState.getColSize(); ++x) {
			for (int y = 0; y < gameState.getRowSize(); ++y) {
				GameObject currentHole = Instantiate (hole, mapMatrixCellToGameField(new Vector2(x, y)),
					Quaternion.identity) as GameObject;
				currentHole.transform.parent = board.transform;
			}
		}
	}

	Vector3 mapMatrixCellToGameField(Vector2 matrixCell) {
		return new Vector3 (matrixCell.x, matrixCell.y, 1);
	}

	GameObject spawnDummyPieceByPlayer(int player) {
		return Instantiate (mapOfPieces [gameState.getCurrentPlayerPieceInfo()],
			new Vector3 (0,  gameState.getRowSize(), 1), Quaternion.identity) as GameObject;
	}

	Vector3 getWorldPositionByMouse() {
		return Camera.main.ScreenToWorldPoint (Input.mousePosition);
	}

	IEnumerator dropPiece(GameObject piece) {
		Vector3 pieceCurrentPosition = piece.transform.position;
		int selectedColumn = Mathf.RoundToInt (pieceCurrentPosition.x);
		int selectedRow = gameLogic.findEmptyRowForColumn (selectedColumn);
		if (selectedRow == -1) {
			isPieceDropping = false;
			isMouseButtonPressed = false;

			yield break;
		} else {
			GameObject newPiece = Instantiate (piece) as GameObject;
			piece.GetComponent<Renderer> ().enabled = false;

			Vector3 sourcePosition = newPiece.transform.position;
			Vector3 targetPosition = new Vector3 (selectedColumn, selectedRow, 1);

			//https://gamedev.stackexchange.com/questions/121318/moving-object-to-point-a-to-point-b-smoothly
			float timeSpent = 0f;
			while(timeSpent < 1f)
			{
				timeSpent += Time.deltaTime;
				newPiece.transform.position = Vector3.Lerp(sourcePosition, targetPosition,
					Mathf.SmoothStep(0f, 1f, timeSpent)
				);

				yield return null;
			}

			gameState.updateCellForCurrentPlayerAt (selectedRow, selectedColumn);
			DestroyImmediate (piece);
			gameLogic.updateGameStateAfterMove ();
		}

		isPieceDropping = false;
		isMouseButtonPressed = false;

		yield return 0;
	}

	string getGameEndText() {
		switch (gameState.getGameStatus()) {
		case BoardConstants.GameStatus.draw :
			return "Game Draw!!";
		case BoardConstants.GameStatus.bluePiece :
			return "Player 1 Won!";
		case BoardConstants.GameStatus.redPiece :
			return "Player 2 Won!";
		}

		return "Error!!";
	}

	// Update is called once per frame
	void Update () {
		if (!gameState.isGameOver()) {
			if (dummyPiece == null) {
				dummyPiece = spawnDummyPieceByPlayer (gameState.getCurrentPlayer());
			} else {
				gameLogic.updatePiecePositionByMousePosition (
					dummyPiece,
					getWorldPositionByMouse ()
				);
				if (Input.GetMouseButtonDown (0) && !isMouseButtonPressed && !isPieceDropping) {
					isPieceDropping = true;
					StartCoroutine (dropPiece (dummyPiece));
				}
			}
		} else {
			gameEndText.GetComponent<Text> ().text = getGameEndText ();
			gameEndText.SetActive (true);

			restartButton.SetActive (true);
		}
	}
}
