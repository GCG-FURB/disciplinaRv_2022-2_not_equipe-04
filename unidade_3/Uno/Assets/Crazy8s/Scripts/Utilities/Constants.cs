using UnityEngine;
using System;
using System.Linq;

public class Constants {
	public const int CARD_SETS = 1;
	public const int CARD_DECKS= 2;
	public const int CARDS_IN_DECK = 56;
	public const int CARDS_IN_COLOR = 14;
	public const int CARDS_COLORS = 4;
	public const int CARDS_IN_START_HAND = 10;
	public const int SEED_INCREASE_STEP = 10000;
	public const int MAX_AVATAR_ID = 4;

	public const float MOVE_ANIM_TIME = 0.33f;
	public const float SOUND_LOCK_TIME = 0.075f;
	public const float CARD_GROW_ANIM_SPEED = 0.33f;
	public const float DEAL_ANIM_TIME = 0.3f;
	public const float TIME_BETWEEN_DEAL_CARD = 0.15f;
	public const float PLANE_DISTACE_OF_CANVAS = 100f;
	public const float QUICK_ANIM_TIME = 0.25f;
	public const float DRAG_STRENGTH = 0.33f;
	public const float DISCARDPILE_CARD_MAX_ANGLE = 15f;
	public const float DRAG_MAX_ANGLE = 18f;
	public const float BOT_ACTION_MIN_DELAY = 1f;
	public const float BOT_ACTION_MAX_DELAY = 4.5f;
	public const float LOCK_HAND_TIME = 0.5f;
	public const float TIME_PER_TURN = 10f;
	public const float AUTOTURN_DELAY = 0.33f;
	public const float FAN_ANGLE = 45;
	public const float BOT_FAN_ANGLE = 30;
	public const float TAP_ZOOM_CARD_SIZE = 1.15f;
	public const float SELECTED_MOVE_CARD_HEIGHT = 50f;
	public const float DRAG_CARD_Z_DISTANCE = -150f;
	public const float DRAG_CARD_Z_ANIM_TIME = 0.5f;



	public static readonly Vector2 vectorZero = new Vector2(0, 0);
	public static readonly Vector2 vectorHalf = new Vector2(0.5f, 0.5f);
	public static readonly Vector3 vectorRight = new Vector3(1,0,0);
	public static readonly Vector3 vectorForward = new Vector3(0, 0, 1);
	public static readonly Vector3 vectorOne = new Vector3(1,1,1);
	public static readonly Vector3 vectorZoom = new Vector3(1.1f, 1.1f, 0);
	public static readonly Vector2 cardSize = new Vector2(190, 271.25f);
	public static readonly Vector3 HAND_CARDS_OFFSET = new Vector3(40f,100f,0);
	public static readonly Color disabledCardColor = new Color32(88, 86, 86, 255);
	public static readonly Color highlightElementColor = new Color32(196, 255, 62, 255);
	public static readonly Color disabledElementColor = new Color32(200, 200, 200, 128);

	public static readonly Quaternion quaternionIdentity = Quaternion.identity;

	public static readonly Color invisibleColor = new Color(0, 0, 0, 0);

	public static readonly string[] namesArray = new string[]
	{
		"MadDog", "McFly", "Commodore", "PrzemekPL", "Kakashi0", "CardMasterX",	"Ugoro", "xXMasterXx", "SLiceNDice", "Worm", "CytrynianPotasu",
		"Luihver", "Amande32", "Sashor", "Rakuchu", "Woogypoo", "Goof01", "Aliness90", "Quintina", "Karlen",
		"Michaella", "Ratclif", "PinkFlamingos", "Rifleman", "Ritos", "Nikalia", "Flashback1984", "IvanMedviec",
		 "Pokemod", "BeerKing", "LazZzerr", "MasterDisaster", "Luxor", "Beagele", "JuKes", "Nutzzz",
		"BlueJoker", "RasherYeah", "Mr.Bean", "Quattro79", "WilliMillie", "McKing", "Theoroie", "Zadok", "Harriet", "Aurinda",
		"RebekahP", "Suitor", "SherlockHolmes", "BigRey", "AwesomeGuy", "Raylynth", "Keturah", "Eowh", "Moffet", "ToxicPrince",
		 "RearAdmiral", "StorageAdmitral", "Tobiasse", "Kaegord", "Abijah"
	};

}