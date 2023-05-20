﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI Instance;

    public static string currentFenInputFieldValue;

    private static bool _isSettingsActive = true;
    public static bool IsSettingsActive
    {
        get
        {
            return _isSettingsActive;
        }
        set
        {   
            _isSettingsActive = value;
            Instance.SettingsGameObject.SetActive(value);
        }
    }
    
    private static bool _pawnPromotionInProgress = false;
    public static bool PawnPromotionInProgress
    {
        get
        {
            return _pawnPromotionInProgress;
        }
        set
        {
            if (value)
            {
                for(int i = 1; i < Instance.PawnPromotionGUI.transform.childCount; i++)
                {
                    GameObject selection = Instance.PawnPromotionGUI.transform.GetChild(i).gameObject;
                    Piece.PieceType type;
                    switch (selection.name)
                    {
                        case "Bishop":
                            type = Piece.PieceType.Bishop;
                            break;
                        case "Knight":
                            type = Piece.PieceType.Knight;
                            break;
                        case "Rook":
                            type = Piece.PieceType.Rook;
                            break;
                        case "Queen":
                            type = Piece.PieceType.Queen;
                            break;
                        default:
                            throw new Exception();
                    }
                    selection.GetComponent<Image>().sprite = Pawn.pawnToBePromoted.color == Piece.PieceColor.White ? Board.PieceTypeToSO[type].whitePieceSprite : Board.PieceTypeToSO[type].blackPieceSprite;
                }
            }
            else
            {
                Pawn.pawnToBePromoted = null;
            }

            _pawnPromotionInProgress = value;
            Instance.PawnPromotionGUI.SetActive(value);
            IsSettingsActive = !value;
        }
    }

    [SerializeField] private GameObject SettingsGameObject;
    [SerializeField] private GameObject RandomPositionSettingsGameObject;
    [SerializeField] private Toggle[] CheckmarkToggles;
    [SerializeField] private Toggle[] RulesToggles;
    [SerializeField] private Text LegalMovesDisplay;
    [SerializeField] private GameObject PawnPromotionGUI;


    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (!PawnPromotionInProgress && Input.GetKeyDown(KeyCode.Tab))
        {
            Board.Instance.spritesParent.SetActive(!Board.Instance.spritesParent.activeInHierarchy);
            LegalMovesDisplay.gameObject.SetActive(!LegalMovesDisplay.gameObject.activeInHierarchy);
            IsSettingsActive = !IsSettingsActive;

            RandomPositionSettingsGameObject.SetActive(!RandomPositionSettingsGameObject.activeInHierarchy);
        }
    }

    public static List<char> GetToggledPieces()
    {
        List<char> validCharacters = new List<char>();

        string characters = "PNBRQKpnbrqk";

        for (int i = 0; i < Instance.CheckmarkToggles.Length; i++)
        {
            if (Instance.CheckmarkToggles[i].isOn)
            {
                validCharacters.Add(characters[i]);
            }
        }

        print("valid characters: " + new string(validCharacters.ToArray()));
        return validCharacters;
    }

    public void UpdateFen(string newFen)
    {
        currentFenInputFieldValue = newFen;
    }

    public void GenerateFenButtonMethod()
    {
        if (currentFenInputFieldValue != null && Board.IsFenValid(currentFenInputFieldValue))
        {
            MoveManager.ResetSelection();
            Board.CreatePositionFromFen(currentFenInputFieldValue);
        }
        else
        {
            print("FEN is invalid! Change it.");
        }        
    }

    /// <summary>
    /// Updates the game settings checkboxes (like allowCheck, allowEnPassant, and allowCastling)
    /// </summary>
    public void UpdateGameSettings()
    {
        MoveManager.CastleAllowed = RulesToggles[0].isOn;
        MoveManager.CheckAllowed = RulesToggles[1].isOn;
        MoveManager.EnPassantAllowed = RulesToggles[2].isOn;
        MoveManager.PawnPromotionAllowed = RulesToggles[3].isOn;
    }

    public static void UpdateLegalMovesDisplay(List<int> legalMoves, bool isPinned)
    {
        string x = $"({legalMoves.Count} legal moves)\n";
        foreach (int move in legalMoves)
        {
            x += Square.SquareNumberToAlphaNumeric(move).ToUpper() + "\n";
        }
        x += $"\n This piece {(isPinned ? "is" : "is not")} pinned";
        Instance.LegalMovesDisplay.text = x;
    }

    public void SelectPawnPromotion(PieceTypeSO pieceTypeSO)
    {
        Pawn.pawnToBePromoted.Promote(pieceTypeSO.pieceType);
        PawnPromotionInProgress = false;
    }

    public void Reset()
    {
        for (int i = 0; i < Instance.RulesToggles.Length; i++)
        {
            Instance.RulesToggles[i].isOn = true;
        }
    }
}
