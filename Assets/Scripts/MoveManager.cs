﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveManager : MonoBehaviour
{
    public static MoveManager Instance { get; private set; }

    private static Piece.PieceColor playerTurn;
    public static Piece.PieceColor PlayerTurn
    {
        get => playerTurn;
        set
        {
            playerTurn = value;

            UI.Instance.ToggleTurnText.text = value.ToString();

            if (AutoChangeView) UI.ChangeBoardView(playerTurn);
        }
    }

    private static bool pieceSelected = false;
    private static Piece selectedPiece = null;
    private static List<int> selectedPieceLegalMoves = null;

    private static List<Piece> checkers = null;
    public static List<Piece> Checkers
    {
        get => checkers;
        set
        {
            checkers = value;
        }
    }

    public static bool CastleAllowed { get; set; } = true;
    public static bool CheckAllowed { get; set; } = true;
    public static bool EnPassantAllowed { get; set; } = true;
    public static bool PawnPromotionAllowed { get; set; } = true;

    private static bool _autoChangeView;
    public static bool AutoChangeView 
    {
        get => _autoChangeView;
        set
        {
            _autoChangeView = value;

            if (UI.Instance?.autoChangeViewToggle != null)
            {
                UI.Instance.autoChangeViewToggle.isOn = value;
            }
        }
    }

    public static bool GameOver { get; set; } = false;


    public LayerMask squaresLayer;
    [SerializeField] private GameObject audioSources;

    void Start()
    {
        if (Instance != null) Destroy(this);

        Instance = this;

        PlayerTurn = Piece.PieceColor.White;
    }

    void Update()
    {
        if (!GameOver && !UI.PawnPromotionInProgress && !Bot.BotMovementInProgress)
        {
            if (Board.gameState == Board.GameState.Bot && playerTurn == Bot.color)
            {
                //if game mode is 1v1 against bot and it is the bot's turn
                StartCoroutine(Bot.PlayBestMove());
            }
            else
            {
                CheckForPieceSelectionOrMove(PlayerTurn);
            }
        }

    }

    public static void CheckForPieceSelectionOrMove(Piece.PieceColor color)
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ResetSquareHighlighting();
            RaycastHit2D hit = Physics2D.Raycast(Utils.GetMouseWorldPosition(), Vector3.zero, 10, Instance.squaresLayer);
            
            if (hit.collider != null) //if we actually hit something
            {
                Square squareHit = hit.collider.GetComponent<Square>();

                if (squareHit.piece?.color == color)
                {
                    pieceSelected = true;
                    selectedPiece = squareHit.piece;
                    selectedPieceLegalMoves = selectedPiece.GetLegalMoves();

                    HighlightLegalSquares(squareHit.SquareNumber, selectedPieceLegalMoves);
                    UI.UpdateLegalMovesDisplay(selectedPieceLegalMoves, selectedPiece.isPinned, squareHit.piece);
                }
                else if (pieceSelected) //checking for move play
                {
                    if (selectedPiece.GetLegalMoves().Contains(squareHit.SquareNumber)) //seeing if the destination square is a legal square that the piece can move to
                    {
                        PlayMove(selectedPiece, squareHit);
                        PlayerTurn = Piece.GetOppositeColor(PlayerTurn);
                        
                        ResetSelection();

                        #region
                        /*if (playerTurn == Piece.PieceColor.White)
                        {
                            playerTurn = Piece.PieceColor.Black;
                            WhiteKingUnderCheck = false; //since white played a move, they must now be out of check, if they were

                            if (CheckForStalemate(Piece.PieceColor.Black))
                            {
                                if (BlackKingUnderCheck) print("CHECKMATE, BLACK HAS NO LEGAL MOVES");
                                else print("STALEMATE, BLACK HAS NO LEGAL MOVES");
                            }
                        }
                        else
                        {
                            playerTurn = Piece.PieceColor.White;
                            BlackKingUnderCheck = false; //since black played a move, they must now be out of check, if they were

                            if (CheckForStalemate(Piece.PieceColor.White))
                            {
                                if (WhiteKingUnderCheck)
                                {
                                    print("CHECKMATE, WHITE HAS NO LEGAL MOVES");
                                }
                                else
                                {
                                    print("STALEMATE, WHITE HAS NO LEGAL MOVES");
                                }
                            }
                        }*/
                        #endregion
                    }
                }
                
            }
        }
        /*else if (Input.GetKey(KeyCode.Mouse0))
        {

        }*/
    }

    public static void PlayMove(Piece piece, Square targetSquare)
    {
        if (targetSquare.piece == null)
        {
            //PlayMoveSound();
        }
        else
        {
            PlayCaptureSound();
            Destroy(targetSquare.piece.gameObj);
        }

        piece.Move(targetSquare);
    }

    public static void PlayMoveSound()
    {
        Instance.audioSources.transform.Find("Move").GetComponent<AudioSource>().Play();
    }
    public static void PlayCaptureSound()
    {
        Instance.audioSources.transform.Find("PieceCapture").GetComponent<AudioSource>().Play();
    }

    public static void DestroyPiece(Piece piece)
    {
        Destroy(piece.gameObj);
    }

    public static bool CheckForStalemate(Piece.PieceColor color)
    {
        return King.kingPieceUnderCheck is null && Piece.GetAllLegalMoves(color) is null;
    }

    //not really highlighting, just "drawing" a point over the square
    public static void HighlightLegalSquares(int selectedSquare, List<int> legalMoves)
    {
        Board.Squares[selectedSquare].Color = Square.SquareColor.Highlighted;

        if (legalMoves != null)
        {
            foreach (int move in legalMoves)
            {
                Board.Squares[move].transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }
    public static void ResetSquareHighlighting()
    {
        foreach (Square square in Board.Squares.Values)
        {
            square.Color = square.Color;
            square.transform.GetChild(0).gameObject.SetActive(false);
        }
    }
    public static void ResetSelection()
    {
        pieceSelected = false;
        selectedPiece = null;
        selectedPieceLegalMoves = null;
        ResetSquareHighlighting();
    }

    public static void ResetFields()
    {
        /*foreach (System.Reflection.FieldInfo fieldInfo in Instance.GetType().GetFields())
        {
            fieldInfo.SetValue(fieldInfo.FieldType, default(fieldInfo.FieldType));
        }*/

        PlayerTurn = Piece.PieceColor.White;
        pieceSelected = false;
        selectedPiece = null;
        selectedPieceLegalMoves = null;
        Checkers = new List<Piece>();

        CastleAllowed = true;
        CheckAllowed = true;
        EnPassantAllowed = true;

        GameOver = false;
    }
}
