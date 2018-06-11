using System;

public static class BoardConstants {
	public enum CellStatus {
		empty,
		bluePiece,
		redPiece
	}

	public enum GameStatus {
		gameRunning,
		bluePiece,
		redPiece,
		draw
	}
}
