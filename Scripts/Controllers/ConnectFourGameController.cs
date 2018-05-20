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

	private Dictionary<GameComponents, GameObject> mapOfPieces;
	private GameComponents[] playerPieceInfo;
	private MatrixStatus[] matrixInfoByPlayer;
	private WinningStatus[] winningInfoByPlayer;

	private int rowSize;
	private int colSize;
	private int player;
	private WinningStatus winner;
	private MatrixStatus[,] boardMatrix;

	private bool isGameOver;
	private bool isMouseButtonPressed;
	private bool isPieceDropping;
	private bool isCheckingForWinner;

	enum GameComponents {
		board,
		hole,
		bluePiece,
		redPiece,
		GameEndText,
		RestartButton
	}

	enum MatrixStatus {
		empty,
		bluePiece,
		redPiece
	}

	enum WinningStatus {
		gameRunning,
		bluePiece,
		redPiece,
		draw
	}

	void Awake() {
		this.colSize = 7;
		this.rowSize = 6;
		this.isGameOver = false;
		this.player = 0;
		this.playerPieceInfo = new GameComponents[] {GameComponents.bluePiece, GameComponents.redPiece};

		// Map game Objects with enum constants.
		this.mapOfPieces = new Dictionary<GameComponents, GameObject> ();
		this.mapOfPieces.Add (GameComponents.bluePiece,
			Resources.Load (GameComponents.bluePiece.ToString ()) as GameObject);
		this.mapOfPieces.Add (GameComponents.redPiece,
			Resources.Load (GameComponents.redPiece.ToString ()) as GameObject);

		// Intialize board Matrix
		this.boardMatrix = new MatrixStatus[rowSize, colSize];

		this.matrixInfoByPlayer = new MatrixStatus[]{ MatrixStatus.bluePiece, MatrixStatus.redPiece };
		this.winningInfoByPlayer = new WinningStatus[]{ WinningStatus.bluePiece, WinningStatus.redPiece };

		this.isMouseButtonPressed = false;
		this.isPieceDropping = false;

		this.winner = WinningStatus.gameRunning;

		this.gameEndText = GameObject.Find (GameComponents.GameEndText.ToString ());
		this.gameEndText.SetActive (false);

		this.restartButton = GameObject.Find (GameComponents.RestartButton.ToString ());
		this.restartButton.SetActive (false);
		this.restartButton.GetComponent<Button>().onClick.AddListener(() => {Utils.SceneUtils.restartGame();});
	}

	// Use this for initialization
	void Start () {
		createBoard ();
		setCameraWithBoardSize ();
		setGameStatusText();
		setRestartButton ();
	}

	void setGameStatusText() {
		this.gameEndText.transform.position = new Vector3 (
			(this.colSize - 1) / 2.0f, ((this.rowSize - 1) / 2.0f),
			1
		);
	}

	void setRestartButton() {
		this.restartButton.transform.position = new Vector3 (this.rowSize, -1, 1);
	}

	void createBoard() {
		this.board = Resources.Load (GameComponents.board.ToString ()) as GameObject;
		this.board = Instantiate (this.board, new Vector3 (0, 0, 10), Quaternion.identity) as GameObject;

		GameObject hole = Resources.Load (GameComponents.hole.ToString ()) as GameObject;
		for (int x = 0; x < this.colSize; ++x) {
			for (int y = 0; y < this.rowSize; ++y) {
				GameObject currentHole = Instantiate (hole, mapMatrixCellToGameField(new Vector2(x, y)),
					Quaternion.identity) as GameObject;
				currentHole.transform.parent = this.board.transform;
				this.boardMatrix [y, x] = MatrixStatus.empty;
			}
		}
	}

	Vector3 mapMatrixCellToGameField(Vector2 matrixCell) {
		return new Vector3 (matrixCell.x, matrixCell.y, 1);
	}

	public int getBoardWidth() {
		return this.colSize;
	}

	void setCameraWithBoardSize() {
		Camera.main.transform.position = new Vector3(
			(this.colSize - 1) / 2.0f, ((this.rowSize - 1) / 2.0f),
			Camera.main.transform.position.z
		);
	}

	GameComponents getPieceEnumByPlayer(int player) {
		return this.playerPieceInfo [player];
	}

	GameObject spawnDummyPieceByPlayer(int player) {
		return Instantiate (mapOfPieces [this.playerPieceInfo [player]],
			new Vector3 (0,  rowSize, 1), Quaternion.identity) as GameObject;
	}

	Vector3 getWorldPositionByMouse() {
		return Camera.main.ScreenToWorldPoint (Input.mousePosition);
	}

	int findEmptyRowForColumn(int column) {
		for (int currentRow = 0; currentRow < this.rowSize; ++currentRow) {
			if (this.boardMatrix [currentRow, column].CompareTo(MatrixStatus.empty) == 0) {
				return currentRow;
			}
		}
		return -1;
	}

	bool boundCheck(int row, int col) {
		if(row >= rowSize || row < 0 || col >= colSize || col < 0) {
			return false;
		}
		return true;
	}

	bool checkForHorizontalMatching(int row, int col, int currentPositionOfPiece) {
		MatrixStatus pieceStatus = this.boardMatrix[row, col];
		// First half
		for (int currentCol = col - currentPositionOfPiece; currentCol < col; ++currentCol) {
			if (!boundCheck(row, currentCol) || this.boardMatrix [row, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentCol = col + (3 - currentPositionOfPiece); currentCol > col; --currentCol) {
			if (!boundCheck(row, currentCol) || this.boardMatrix [row, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForVerticalMatching(int row, int col, int currentPositionOfPiece) {
		MatrixStatus pieceStatus = this.boardMatrix[row, col];
		// First half
		for (int currentRow = row - currentPositionOfPiece; currentRow < row; ++currentRow) {
			if (!boundCheck (currentRow, col) || this.boardMatrix [currentRow, col].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece); currentRow > row; --currentRow) {
			if (!boundCheck (currentRow, col) || this.boardMatrix [currentRow, col].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForLeftToRightDiagonalMatching(int row, int col, int currentPositionOfPiece) {
		MatrixStatus pieceStatus = this.boardMatrix[row, col];

		//First half
		for (int currentRow = row - currentPositionOfPiece, currentCol = col - currentPositionOfPiece;
			currentRow < row && currentCol < col; ++currentRow, ++currentCol) {
			if (!boundCheck (currentRow, currentCol) || this.boardMatrix [currentRow, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece), currentCol = col + (3 - currentPositionOfPiece);
			currentRow > row && currentCol > col; --currentRow, --currentCol) {
			if (!boundCheck (currentRow, currentCol) || this.boardMatrix [currentRow, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForRightToLeftDiagonalMatching(int row, int col, int currentPositionOfPiece) {
		MatrixStatus pieceStatus = this.boardMatrix[row, col];

		//First half
		for (int currentRow = row - currentPositionOfPiece, currentCol = col + currentPositionOfPiece;
			currentRow < row && currentCol > col; ++currentRow, --currentCol) {
			if (!boundCheck (currentRow, currentCol) || this.boardMatrix [currentRow, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece), currentCol = col - (3 - currentPositionOfPiece);
			currentRow > row && currentCol < col; --currentRow, ++currentCol) {
			if (!boundCheck (currentRow, currentCol) || this.boardMatrix [currentRow, currentCol].CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForDiagonalMatching(int row, int col, int currentPositionOfPiece) {
		return checkForLeftToRightDiagonalMatching (row, col, currentPositionOfPiece)
		|| checkForRightToLeftDiagonalMatching (row, col, currentPositionOfPiece);
	}

	bool isWinningPosition(int row, int col) {
		// Right now our game logic implies that if 4 same pieces connected then it will be
		// win for that color player. Here 'currentPositionOfPiece' is holding information
		// that this piece will be on which position if there are 4 same pieces connected.
		bool winning = false;
		for (int currentPositonOfPiece = 0; currentPositonOfPiece < 4; ++currentPositonOfPiece) {
			winning |= checkForHorizontalMatching (row, col, currentPositonOfPiece) 
				|| checkForVerticalMatching (row, col, currentPositonOfPiece)
				|| checkForDiagonalMatching (row, col, currentPositonOfPiece);
		}

		return winning;
	}

	void checkForWinner() {
		bool anyWinning = false;
		for (int row = 0; row < this.rowSize; ++row) {
			for (int col = 0; col < this.colSize; ++col) {
				if (this.boardMatrix [row, col].CompareTo (MatrixStatus.empty) != 0) {
					bool win =  isWinningPosition (row, col);
					if (win) {
						this.winner = this.winningInfoByPlayer[this.player];
						this.isGameOver = true;
						Debug.Log ("Game Over winner is : " + winner);
					}

					anyWinning |= win;
				}
			}
		}

		this.isCheckingForWinner = false;
	}

	bool isDraw() {
		for (int row = 0; row < rowSize; ++row) {
			for (int col = 0; col < colSize; ++col) {
				if (this.boardMatrix [row, col].CompareTo (MatrixStatus.empty) == 0) {
					return false;
				}
			}
		}

		return true;
	}

	IEnumerator dropPiece(GameObject piece) {
		Vector3 pieceCurrentPosition = piece.transform.position;
		int selectedColumn = Mathf.RoundToInt (pieceCurrentPosition.x);
		int selectedRow = findEmptyRowForColumn (selectedColumn);
		if (selectedRow == -1) {
			// Not Valid Move
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

			this.boardMatrix [selectedRow, selectedColumn] = this.matrixInfoByPlayer [this.player];
		
			DestroyImmediate (piece);

			this.isCheckingForWinner = true;
			checkForWinner ();

			if (this.winner.CompareTo(WinningStatus.gameRunning) == 0 && isDraw()) {
				this.winner = WinningStatus.draw;
			}
		}

		this.isPieceDropping = false;
		this.isMouseButtonPressed = false;
		this.player = 1 - this.player;

		yield return 0;
	}

	string getGameEndText() {
		switch (this.winner) {
		case WinningStatus.draw :
			return "Game Draw!!";
		case WinningStatus.bluePiece :
			return "Player 1 Won!";
		case WinningStatus.redPiece :
			return "Player 2 Won!";
		}
		return "Error!!";
	}

	// Update is called once per frame
	void Update () {
		if (!this.isGameOver) {
			if (this.dummyPiece == null) {
				this.dummyPiece = spawnDummyPieceByPlayer (player);
			} else {
				Vector3 mouseCurrentPosition = getWorldPositionByMouse ();
				this.dummyPiece.transform.position = new Vector3 (
					Mathf.Clamp (mouseCurrentPosition.x, 0, this.colSize - 1), this.rowSize, 1);
				if (Input.GetMouseButtonDown (0) && !this.isMouseButtonPressed && !this.isPieceDropping) {
					this.isPieceDropping = true;
					StartCoroutine (dropPiece (this.dummyPiece));
				}
			}
		} else {
			this.gameEndText.GetComponent<Text> ().text = getGameEndText ();
			this.gameEndText.SetActive (true);

			this.restartButton.SetActive (true);
		}
	}
}
