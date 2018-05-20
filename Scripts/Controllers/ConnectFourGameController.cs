using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectFourGameController : MonoBehaviour {

	private GameObject board;
	private GameObject dummyPiece;

	private Dictionary<GameComponents, GameObject> mapOfPieces;
	private GameComponents[] playerPieceInfo;
	private MatrixStatus[] matrixInfoByPlayer;

	private int rowSize;
	private int colSize;
	private int player;
	private MatrixStatus[,] boardMatrix;

	private float speed;

	private bool isGameOver;
	private bool isMouseButtonPressed;
	private bool isPieceDropping;

	enum GameComponents {
		board,
		hole,
		bluePiece,
		redPiece
	}

	enum MatrixStatus {
		empty,
		bluePiece,
		redPiece
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

		this.isMouseButtonPressed = false;
		this.isPieceDropping = false;

		this.speed = 2.0f;
	}

	// Use this for initialization
	void Start () {
		createBoard ();
		setCameraWithBoardSize ();
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
			(this.colSize - 1) / 2.0f, ((this.rowSize - 1) / 2.0f), Camera.main.transform.position.z);
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
			if (boardMatrix [currentRow, column].CompareTo(MatrixStatus.empty) == 0) {
				return currentRow;
			}
		}
		return -1;
	}

	IEnumerator dropPiece(GameObject piece) {
		this.isPieceDropping = true;

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
			float steps = speed * Time.deltaTime;

			float t = 0f;
			while(t < 1f)
			{
				t += Time.deltaTime;
				newPiece.transform.position = Vector3.Lerp(sourcePosition, targetPosition, Mathf.SmoothStep(0f, 1f, t));
				yield return null;
			}
			//newPiece.transform.position = Vector3.MoveTowards (sourcePosition, targetPosition, steps);

			this.boardMatrix [selectedRow, selectedColumn] = this.matrixInfoByPlayer [this.player];
			this.player = 1 - this.player;

			DestroyImmediate (piece);

		}

		this.isPieceDropping = false;
		this.isMouseButtonPressed = false;

		yield return 0;
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
					StartCoroutine (dropPiece (this.dummyPiece));
				}
			}
		} else {
			// No move
		}
	}
}
