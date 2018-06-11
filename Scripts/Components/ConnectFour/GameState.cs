using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState {

	private int rowSize;
	private int colSize;
	private int currentPlayer;

	private bool gameOver;

	private UIConstants.Components[] playerPieceInfo;
	private BoardConstants.CellStatus[] matrixInfoByPlayer;
	private BoardConstants.GameStatus[] winningInfoByPlayer;
	private BoardConstants.GameStatus gameStatus;

	private BoardConstants.CellStatus[,] boardMatrix;

	public GameState() {
		this.playerPieceInfo = 
			new UIConstants.Components[] {
				UIConstants.Components.bluePiece,
				UIConstants.Components.redPiece
		};

		this.matrixInfoByPlayer = 
			new BoardConstants.CellStatus[]{
				BoardConstants.CellStatus.bluePiece,
				BoardConstants.CellStatus.redPiece
		};

		this.winningInfoByPlayer = 
			new BoardConstants.GameStatus[]{
				BoardConstants.GameStatus.bluePiece,
				BoardConstants.GameStatus.redPiece
		};

		this.gameStatus = BoardConstants.GameStatus.gameRunning;
		this.gameOver = false;
	}

	public int getRowSize() {
		return rowSize;
	}

	public int getColSize() {
		return colSize;
	}

	public int getCurrentPlayer() {
		return currentPlayer;
	}

	public BoardConstants.CellStatus getCellStatusAt(int row, int col) {
		return boardMatrix [row, col];
	}

	public BoardConstants.GameStatus getGameStatus() {
		return gameStatus;
	}

	public bool isGameOver() {
		return gameOver;
	}

	public void intializeBoard(int rowSize, int colSize) {
		this.rowSize = rowSize;
		this.colSize = colSize;

		boardMatrix = new BoardConstants.CellStatus[rowSize, colSize];
		for (int row = 0; row < rowSize; ++row) {
			for (int col = 0; col < colSize; ++col) {
				boardMatrix [row, col] = BoardConstants.CellStatus.empty;
			}
		}
	}

	public void updateCellForCurrentPlayerAt(int row, int col) {
		this.boardMatrix [row, col] = matrixInfoByPlayer[currentPlayer];
	}

	public UIConstants.Components getCurrentPlayerPieceInfo() {
		return playerPieceInfo [currentPlayer];
	}

	public void winningMovePlayed() {
		gameStatus = winningInfoByPlayer [currentPlayer];
		gameOver = true;
	}

	public void gameDraw() {
		gameStatus = BoardConstants.GameStatus.draw;
	}

	public void nextPlayerChance() {
		currentPlayer = 1 - currentPlayer;
	}
}
