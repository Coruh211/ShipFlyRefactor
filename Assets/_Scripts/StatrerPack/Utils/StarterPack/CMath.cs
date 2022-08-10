using UnityEngine;

public class CMath
{
    public static readonly string[] Digits =
    {
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
        "30", "31", "32", "33", "34", "35", "36", "37", "38", "39",
        "40", "41", "42", "43", "44", "45", "46", "47", "48", "49",
        "50", "51", "52", "53", "54", "55", "56", "57", "58", "59",
        "60", "61", "62", "63", "64", "65", "66", "67", "68", "69",
        "70", "71", "72", "73", "74", "75", "76", "77", "78", "79",
        "80", "81", "82", "83", "84", "85", "86", "87", "88", "89",
        "90", "91", "92", "93", "94", "95", "96", "97", "98", "99", "100"
    };

    public static readonly Vector2 Up2 = new Vector2(0, 1);
    public static readonly Vector2 Down2 = new Vector2(0, -1);
    public static readonly Vector2 Left2 = new Vector2(-1, 0);
    public static readonly Vector2 Right2 = new Vector2(1, 0);
    public static readonly Vector2 Zero2 = new Vector2(0, 1);
    public static readonly Vector2 One2 = new Vector2(0, 1);
    
    public static readonly Vector3 Up = new Vector3(0, 1, 0);
    public static readonly Vector3 Down = new Vector3(0, -1, 0);
    public static readonly Vector3 Left = new Vector3(-1, 0, 0);
    public static readonly Vector3 Right = new Vector3(1, 0, 0);
    public static readonly Vector3 Forward = new Vector3(0, 0, 1);
    public static readonly Vector3 Back = new Vector3(0, 0, -1);
    public static readonly Vector3 Zero = new Vector3(0, 0, 0);
    public static readonly Vector3 One = new Vector3(1, 1, 1);
}