using System;
using UnityEngine;

public class ConnectFourGameLogic {

	private GameState currentState;

	public ConnectFourGameLogic (GameState gameState) {
		currentState = gameState;
	}

	public void movePlayedAt(int row, int col) {
		currentState.updateCellForCurrentPlayerAt (row, col);
	}

	public void updatePiecePositionByMousePosition(GameObject piece,
			Vector3 mousePosition) {
		piece.transform.position =
			new Vector3 (
				Mathf.Clamp (mousePosition.x, 0, currentState.getColSize() - 1),
				currentState.getRowSize(),
				1
			);
	}

	public int suitableColumnForPiece(GameObject piece) {
		Vector3 pieceCurrentPosition = piece.transform.position;
		return Mathf.RoundToInt (pieceCurrentPosition.x);
	}

	public int avilableRowForPieceByColumn(int col) {
		return findEmptyRowForColumn (col);
	}

	public void updateGameStateAfterMove() {
		bool isWinner = checkForWinner ();
		if (isWinner) {
			currentState.winningMovePlayed ();
		} else if (isDraw ()) {
			currentState.gameDraw ();
		} else {
			currentState.nextPlayerChance ();
		}
	}

	public int findEmptyRowForColumn(int column) {
		for (int currentRow = 0; currentRow < currentState.getRowSize(); ++currentRow) {
			if (currentState.getCellStatusAt(currentRow, column).CompareTo(BoardConstants.CellStatus.empty) == 0) {
				return currentRow;
			}
		}
		return -1;
	}

	bool boundCheck(int row, int col) {
		if(row >= currentState.getRowSize() || row < 0 || col >= currentState.getColSize() || col < 0) {
			return false;
		}
		return true;
	}

	bool checkForHorizontalMatching(int row, int col, int currentPositionOfPiece) {
		BoardConstants.CellStatus pieceStatus = currentState.getCellStatusAt(row, col);
		// First half
		for (int currentCol = col - currentPositionOfPiece; currentCol < col; ++currentCol) {
			if (!boundCheck(row, currentCol) || currentState.getCellStatusAt(row, currentCol).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentCol = col + (3 - currentPositionOfPiece); currentCol > col; --currentCol) {
			if (!boundCheck(row, currentCol) || currentState.getCellStatusAt(row, currentCol).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForVerticalMatching(int row, int col, int currentPositionOfPiece) {
		BoardConstants.CellStatus pieceStatus = currentState.getCellStatusAt(row, col);
		// First half
		for (int currentRow = row - currentPositionOfPiece; currentRow < row; ++currentRow) {
			if (!boundCheck (currentRow, col) || currentState.getCellStatusAt(currentRow, col).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece); currentRow > row; --currentRow) {
			if (!boundCheck (currentRow, col) || currentState.getCellStatusAt(currentRow, col).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForLeftToRightDiagonalMatching(int row, int col, int currentPositionOfPiece) {
		BoardConstants.CellStatus pieceStatus = currentState.getCellStatusAt(row, col);

		//First half
		for (int currentRow = row - currentPositionOfPiece, currentCol = col - currentPositionOfPiece;
			currentRow < row && currentCol < col; ++currentRow, ++currentCol) {
			if (!boundCheck (currentRow, currentCol) || currentState.getCellStatusAt(currentRow, currentCol).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece), currentCol = col + (3 - currentPositionOfPiece);
			currentRow > row && currentCol > col; --currentRow, --currentCol) {
			if (!boundCheck (currentRow, currentCol) || currentState.getCellStatusAt(currentRow, currentCol).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		return true;
	}

	bool checkForRightToLeftDiagonalMatching(int row, int col, int currentPositionOfPiece) {
		BoardConstants.CellStatus pieceStatus = currentState.getCellStatusAt(row, col);

		//First half
		for (int currentRow = row - currentPositionOfPiece, currentCol = col + currentPositionOfPiece;
			currentRow < row && currentCol > col; ++currentRow, --currentCol) {
			if (!boundCheck (currentRow, currentCol) || currentState.getCellStatusAt(currentRow, currentCol).CompareTo (pieceStatus) != 0) {
				return false;
			}
		}

		// Second half
		for (int currentRow = row + (3 - currentPositionOfPiece), currentCol = col - (3 - currentPositionOfPiece);
			currentRow > row && currentCol < col; --currentRow, ++currentCol) {
			if (!boundCheck (currentRow, currentCol) || currentState.getCellStatusAt(currentRow, currentCol).CompareTo (pieceStatus) != 0) {
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

	public bool isDraw() {
		for (int row = 0; row < currentState.getRowSize(); ++row) {
			for (int col = 0; col < currentState.getColSize(); ++col) {
				if (currentState.getCellStatusAt(row, col).CompareTo (BoardConstants.CellStatus.empty) == 0) {
					return false;
				}
			}
		}

		return true;
	}

	public bool checkForWinner() {
		bool anyWinning = false;
		for (int row = 0; row < currentState.getRowSize(); ++row) {
			for (int col = 0; col < currentState.getColSize(); ++col) {
				if (currentState.getCellStatusAt(row, col).CompareTo (BoardConstants.CellStatus.empty) != 0) {
					bool win =  isWinningPosition (row, col);
					if (win) {
						return true;
					}

					anyWinning |= win;
				}
			}
		}

		return anyWinning;
	}
}
