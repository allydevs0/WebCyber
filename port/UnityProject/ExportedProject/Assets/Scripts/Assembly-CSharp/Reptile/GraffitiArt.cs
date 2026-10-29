using System;
using Reptile.Phone;
using UnityEngine;

namespace Reptile
{
	[Serializable]
	public class GraffitiArt
	{
		public static class Titles
		{
			public const string playersGraf_S0 = "Red";

			public const string franksGraf_S = "‘F’ Is For Family";

			public const string DOTEXEGraf_S = "8O TUP";

			public const string EclipseGraf_S = "Elegant E";

			public const string playersGraf_M0 = "QUICK BING";

			public const string playersGraf_M3 = "Zona Leste";

			public const string playersGraf_M1 = "Stacked Symbols";

			public const string franksGraf_M = "All Screws Loose";

			public const string EclipseGraf_M = "Constellation Circle";

			public const string playersGraf_M5 = "SHOGUN";

			public const string DOTEXEGraf_M = "8 MINUTES OF LEAN MEAN";

			public const string playersGraf_M2 = "BLOCKY";

			public const string playersGraf_M4 = "pico pow";

			public const string playersGraf_M6 = "colorBOMB";

			public const string playersGraf_M13 = "Vom'B";

			public const string playersGraf_M10 = "SPRAYTANICPANIC!";

			public const string playersGraf_M9 = "BOMB BEATS";

			public const string playersGraf_M8 = "Pora";

			public const string playersGraf_M11 = "TeleBinge";

			public const string playersGraf_M12 = "B-boy Love";

			public const string playersGraf_M15 = "Thick Candy";

			public const string playersGraf_M16 = "0m33";

			public const string playersGraf_M14 = "Street classic";

			public const string playersGraf_M7 = "OVERWHELMME";

			public const string playersGraf_M17 = "Devil 68";

			public const string playersGraf_L0 = "WHOLE SIXER";

			public const string playersGraf_L1 = "INFINITY";

			public const string playersGraf_L5 = "NOISY NINJA";

			public const string franksGraf_L = "Dinner On The Court";

			public const string playersGraf_L3 = "Jd Vila Formosa";

			public const string playersGraf_L2 = "Messenger Mural";

			public const string DOTEXEGraf_L = "DIGITAL BLOCKBUSTER";

			public const string playersGraf_L6 = "skate or di3";

			public const string playersGraf_L17 = "VoodooBoy";

			public const string playersGraf_L16 = "Boom";

			public const string playersGraf_L4 = "buttercup";

			public const string EclipseGraf_L = "Solstice Script";

			public const string playersGraf_L10 = "Moai Marathon";

			public const string playersGraf_L15 = "Tius";

			public const string playersGraf_L8 = "FREAKS";

			public const string playersGraf_L9 = "SpawningSeason";

			public const string playersGraf_L7 = "Fang It Up!";

			public const string playersGraf_L11 = "Graffo Le Fou";

			public const string playersGraf_L14 = "RECORD.HEAD";

			public const string playersGraf_L12 = "Campaign Trail";

			public const string playersGraf_L13 = "Lauder";

			public const string playersGraf_XL0 = "WILD STRUXXA";

			public const string playersGraf_XL1 = "VIBRATIONS";

			public const string playersGraf_XL5 = "MOTORCYCLE GANG";

			public const string franksGraf_XL = "Don’t Screw Around";

			public const string playersGraf_XL6 = "MegaHood";

			public const string playersGraf_XL3 = "Gamex UPA ABL";

			public const string playersGraf_XL4 = "headphones on Helmet on";

			public const string playersGraf_XL2 = "Bomb Burner";

			public const string EclipseGraf_XL = "Astrological Augury";

			public const string DOTEXEGraf_XL = "HIGH TECH WS";

			public const string playersGraf_XL10 = "Web Spitter";

			public const string playersGraf_XL8 = "SECOND SIGHT";

			public const string playersGraf_XL9 = "FATE";

			public const string playersGraf_XL12 = "Pirate's Life 4 Me";

			public const string playersGraf_XL13 = "BiGSHiNYBoMB";

			public const string playersGraf_XL11 = "Deep Dive";

			public const string playersGraf_XL14 = "Bombing by FireMan";

			public const string playersGraf_XL7 = "Gold Rush";

			public const string playersGraf_XL15 = "Raver Funk";

			public const string playersGraf_XL16 = "Bomb Croc";

			public const string playersGraf_M18 = "Teddy 4";

			public const string DevilTheoryGraf_M = "EVIL DARUMA";

			public const string DevilTheoryGraf_L = "KANI-BOZU";

			public const string DevilTheoryGraf_S = "DT";

			public const string DevilTheoryGraf_XL = "CYBER TENGU";

			public const string playersGraf_XL17 = "end 2 end";

			public const string playersGraf_L18 = "wild rush";

			public const string playersGraf_S1 = "Felix";

			public const string playersGraf_S10 = "Shine";

			public const string playersGraf_S9 = "Rise";

			public const string playersGraf_S8 = "Faux";

			public const string playersGraf_S7 = "Coil";

			public const string playersGraf_S6 = "Solace";

			public const string playersGraf_S5 = "Rave";

			public const string playersGraf_S4 = "Tryce";

			public const string playersGraf_S3 = "Bel";

			public const string playersGraf_S11 = "Mesh";

			public const string playersGraf_S2 = "Vinyl";

			public const string FUTURISMGraf_M = "Flow";

			public const string FUTURISMGraf_L = "Dynamo";

			public const string FUTURISMGraf_S = "Eye-con";

			public const string FUTURISMGraf_XL = "Bevel";

			public const string playersGraf_XL18 = "Bomb Universe";

			public const string playersGraf_S13 = "Boombap";

			public const string playersGraf_S14 = "Jay";

			public const string playersGraf_S12 = "Irene";

			public const string playersGraf_S15 = "Base";
		}

		public enum Combos
		{
			NONE = -1,
			m1234 = 1234,
			m1243 = 1243,
			m1324 = 1324,
			m1342 = 1342,
			m1423 = 1423,
			m1432 = 1432,
			m2134 = 2134,
			m2143 = 2143,
			m2314 = 2314,
			m2341 = 2341,
			m2413 = 2413,
			m2431 = 2431,
			m3124 = 3124,
			m3142 = 3142,
			m3214 = 3214,
			m3241 = 3241,
			m3412 = 3412,
			m3421 = 3421,
			m4123 = 4123,
			m4132 = 4132,
			m4213 = 4213,
			m4231 = 4231,
			m4312 = 4312,
			m4321 = 4321,
			l12345 = 12345,
			l12354 = 12354,
			l12435 = 12435,
			l12453 = 12453,
			l12534 = 12534,
			l12543 = 12543,
			l13245 = 13245,
			l13254 = 13254,
			l13425 = 13425,
			l13452 = 13452,
			l13524 = 13524,
			l13542 = 13542,
			l14235 = 14235,
			l14253 = 14253,
			l14325 = 14325,
			l14352 = 14352,
			l14523 = 14523,
			l14532 = 14532,
			l15234 = 15234,
			l15243 = 15243,
			l15324 = 15324,
			l15342 = 15342,
			l15423 = 15423,
			l15432 = 15432,
			l21345 = 21345,
			l21354 = 21354,
			l21435 = 21435,
			l21453 = 21453,
			l21534 = 21534,
			l21543 = 21543,
			l23145 = 23145,
			l23154 = 23154,
			l23415 = 23415,
			l23451 = 23451,
			l23514 = 23514,
			l23541 = 23541,
			l24135 = 24135,
			l24153 = 24153,
			l24315 = 24315,
			l24351 = 24351,
			l24513 = 24513,
			l24531 = 24531,
			l25134 = 25134,
			l25143 = 25143,
			l25314 = 25314,
			l25341 = 25341,
			l25413 = 25413,
			l25431 = 25431,
			l31245 = 31245,
			l31254 = 31254,
			l31425 = 31425,
			l31452 = 31452,
			l31524 = 31524,
			l31542 = 31542,
			l32145 = 32145,
			l32154 = 32154,
			l32415 = 32415,
			l32451 = 32451,
			l32514 = 32514,
			l32541 = 32541,
			l34125 = 34125,
			l34152 = 34152,
			l34215 = 34215,
			l34251 = 34251,
			l34512 = 34512,
			l34521 = 34521,
			l35124 = 35124,
			l35142 = 35142,
			l35214 = 35214,
			l35241 = 35241,
			l35412 = 35412,
			l35421 = 35421,
			l41235 = 41235,
			l41253 = 41253,
			l41325 = 41325,
			l41352 = 41352,
			l41523 = 41523,
			l41532 = 41532,
			l42135 = 42135,
			l42153 = 42153,
			l42315 = 42315,
			l42351 = 42351,
			l42513 = 42513,
			l42531 = 42531,
			l43125 = 43125,
			l43152 = 43152,
			l43215 = 43215,
			l43251 = 43251,
			l43512 = 43512,
			l43521 = 43521,
			l45123 = 45123,
			l45132 = 45132,
			l45213 = 45213,
			l45231 = 45231,
			l45312 = 45312,
			l45321 = 45321,
			l51234 = 51234,
			l51243 = 51243,
			l51324 = 51324,
			l51342 = 51342,
			l51423 = 51423,
			l51432 = 51432,
			l52134 = 52134,
			l52143 = 52143,
			l52314 = 52314,
			l52341 = 52341,
			l52413 = 52413,
			l52431 = 52431,
			l53124 = 53124,
			l53142 = 53142,
			l53214 = 53214,
			l53241 = 53241,
			l53412 = 53412,
			l53421 = 53421,
			l54123 = 54123,
			l54132 = 54132,
			l54213 = 54213,
			l54231 = 54231,
			l54312 = 54312,
			l54321 = 54321,
			xl123456 = 123456,
			xl123465 = 123465,
			xl123546 = 123546,
			xl123564 = 123564,
			xl123645 = 123645,
			xl123654 = 123654,
			xl124356 = 124356,
			xl124365 = 124365,
			xl124536 = 124536,
			xl124563 = 124563,
			xl124635 = 124635,
			xl124653 = 124653,
			xl125346 = 125346,
			xl125364 = 125364,
			xl125436 = 125436,
			xl125463 = 125463,
			xl125634 = 125634,
			xl125643 = 125643,
			xl126345 = 126345,
			xl126354 = 126354,
			xl126435 = 126435,
			xl126453 = 126453,
			xl126534 = 126534,
			xl126543 = 126543,
			xl132456 = 132456,
			xl132465 = 132465,
			xl132546 = 132546,
			xl132564 = 132564,
			xl132645 = 132645,
			xl132654 = 132654,
			xl134256 = 134256,
			xl134265 = 134265,
			xl134526 = 134526,
			xl134562 = 134562,
			xl134625 = 134625,
			xl134652 = 134652,
			xl135246 = 135246,
			xl135264 = 135264,
			xl135426 = 135426,
			xl135462 = 135462,
			xl135624 = 135624,
			xl135642 = 135642,
			xl136245 = 136245,
			xl136254 = 136254,
			xl136425 = 136425,
			xl136452 = 136452,
			xl136524 = 136524,
			xl136542 = 136542,
			xl142356 = 142356,
			xl142365 = 142365,
			xl142536 = 142536,
			xl142563 = 142563,
			xl142635 = 142635,
			xl142653 = 142653,
			xl143256 = 143256,
			xl143265 = 143265,
			xl143526 = 143526,
			xl143562 = 143562,
			xl143625 = 143625,
			xl143652 = 143652,
			xl145236 = 145236,
			xl145263 = 145263,
			xl145326 = 145326,
			xl145362 = 145362,
			xl145623 = 145623,
			xl145632 = 145632,
			xl146235 = 146235,
			xl146253 = 146253,
			xl146325 = 146325,
			xl146352 = 146352,
			xl146523 = 146523,
			xl146532 = 146532,
			xl152346 = 152346,
			xl152364 = 152364,
			xl152436 = 152436,
			xl152463 = 152463,
			xl152634 = 152634,
			xl152643 = 152643,
			xl153246 = 153246,
			xl153264 = 153264,
			xl153426 = 153426,
			xl153462 = 153462,
			xl153624 = 153624,
			xl153642 = 153642,
			xl154236 = 154236,
			xl154263 = 154263,
			xl154326 = 154326,
			xl154362 = 154362,
			xl154623 = 154623,
			xl154632 = 154632,
			xl156234 = 156234,
			xl156243 = 156243,
			xl156324 = 156324,
			xl156342 = 156342,
			xl156423 = 156423,
			xl156432 = 156432,
			xl162345 = 162345,
			xl162354 = 162354,
			xl162435 = 162435,
			xl162453 = 162453,
			xl162534 = 162534,
			xl162543 = 162543,
			xl163245 = 163245,
			xl163254 = 163254,
			xl163425 = 163425,
			xl163452 = 163452,
			xl163524 = 163524,
			xl163542 = 163542,
			xl164235 = 164235,
			xl164253 = 164253,
			xl164325 = 164325,
			xl164352 = 164352,
			xl164523 = 164523,
			xl164532 = 164532,
			xl165234 = 165234,
			xl165243 = 165243,
			xl165324 = 165324,
			xl165342 = 165342,
			xl165423 = 165423,
			xl165432 = 165432,
			xl213456 = 213456,
			xl213465 = 213465,
			xl213546 = 213546,
			xl213564 = 213564,
			xl213645 = 213645,
			xl213654 = 213654,
			xl214356 = 214356,
			xl214365 = 214365,
			xl214536 = 214536,
			xl214563 = 214563,
			xl214635 = 214635,
			xl214653 = 214653,
			xl215346 = 215346,
			xl215364 = 215364,
			xl215436 = 215436,
			xl215463 = 215463,
			xl215634 = 215634,
			xl215643 = 215643,
			xl216345 = 216345,
			xl216354 = 216354,
			xl216435 = 216435,
			xl216453 = 216453,
			xl216534 = 216534,
			xl216543 = 216543,
			xl231456 = 231456,
			xl231465 = 231465,
			xl231546 = 231546,
			xl231564 = 231564,
			xl231645 = 231645,
			xl231654 = 231654,
			xl234156 = 234156,
			xl234165 = 234165,
			xl234516 = 234516,
			xl234561 = 234561,
			xl234615 = 234615,
			xl234651 = 234651,
			xl235146 = 235146,
			xl235164 = 235164,
			xl235416 = 235416,
			xl235461 = 235461,
			xl235614 = 235614,
			xl235641 = 235641,
			xl236145 = 236145,
			xl236154 = 236154,
			xl236415 = 236415,
			xl236451 = 236451,
			xl236514 = 236514,
			xl236541 = 236541,
			xl241356 = 241356,
			xl241365 = 241365,
			xl241536 = 241536,
			xl241563 = 241563,
			xl241635 = 241635,
			xl241653 = 241653,
			xl243156 = 243156,
			xl243165 = 243165,
			xl243516 = 243516,
			xl243561 = 243561,
			xl243615 = 243615,
			xl243651 = 243651,
			xl245136 = 245136,
			xl245163 = 245163,
			xl245316 = 245316,
			xl245361 = 245361,
			xl245613 = 245613,
			xl245631 = 245631,
			xl246135 = 246135,
			xl246153 = 246153,
			xl246315 = 246315,
			xl246351 = 246351,
			xl246513 = 246513,
			xl246531 = 246531,
			xl251346 = 251346,
			xl251364 = 251364,
			xl251436 = 251436,
			xl251463 = 251463,
			xl251634 = 251634,
			xl251643 = 251643,
			xl253146 = 253146,
			xl253164 = 253164,
			xl253416 = 253416,
			xl253461 = 253461,
			xl253614 = 253614,
			xl253641 = 253641,
			xl254136 = 254136,
			xl254163 = 254163,
			xl254316 = 254316,
			xl254361 = 254361,
			xl254613 = 254613,
			xl254631 = 254631,
			xl256134 = 256134,
			xl256143 = 256143,
			xl256314 = 256314,
			xl256341 = 256341,
			xl256413 = 256413,
			xl256431 = 256431,
			xl261345 = 261345,
			xl261354 = 261354,
			xl261435 = 261435,
			xl261453 = 261453,
			xl261534 = 261534,
			xl261543 = 261543,
			xl263145 = 263145,
			xl263154 = 263154,
			xl263415 = 263415,
			xl263451 = 263451,
			xl263514 = 263514,
			xl263541 = 263541,
			xl264135 = 264135,
			xl264153 = 264153,
			xl264315 = 264315,
			xl264351 = 264351,
			xl264513 = 264513,
			xl264531 = 264531,
			xl265134 = 265134,
			xl265143 = 265143,
			xl265314 = 265314,
			xl265341 = 265341,
			xl265413 = 265413,
			xl265431 = 265431,
			xl312456 = 312456,
			xl312465 = 312465,
			xl312546 = 312546,
			xl312564 = 312564,
			xl312645 = 312645,
			xl312654 = 312654,
			xl314256 = 314256,
			xl314265 = 314265,
			xl314526 = 314526,
			xl314562 = 314562,
			xl314625 = 314625,
			xl314652 = 314652,
			xl315246 = 315246,
			xl315264 = 315264,
			xl315426 = 315426,
			xl315462 = 315462,
			xl315624 = 315624,
			xl315642 = 315642,
			xl316245 = 316245,
			xl316254 = 316254,
			xl316425 = 316425,
			xl316452 = 316452,
			xl316524 = 316524,
			xl316542 = 316542,
			xl321456 = 321456,
			xl321465 = 321465,
			xl321546 = 321546,
			xl321564 = 321564,
			xl321645 = 321645,
			xl321654 = 321654,
			xl324156 = 324156,
			xl324165 = 324165,
			xl324516 = 324516,
			xl324561 = 324561,
			xl324615 = 324615,
			xl324651 = 324651,
			xl325146 = 325146,
			xl325164 = 325164,
			xl325416 = 325416,
			xl325461 = 325461,
			xl325614 = 325614,
			xl325641 = 325641,
			xl326145 = 326145,
			xl326154 = 326154,
			xl326415 = 326415,
			xl326451 = 326451,
			xl326514 = 326514,
			xl326541 = 326541,
			xl341256 = 341256,
			xl341265 = 341265,
			xl341526 = 341526,
			xl341562 = 341562,
			xl341625 = 341625,
			xl341652 = 341652,
			xl342156 = 342156,
			xl342165 = 342165,
			xl342516 = 342516,
			xl342561 = 342561,
			xl342615 = 342615,
			xl342651 = 342651,
			xl345126 = 345126,
			xl345162 = 345162,
			xl345216 = 345216,
			xl345261 = 345261,
			xl345612 = 345612,
			xl345621 = 345621,
			xl346125 = 346125,
			xl346152 = 346152,
			xl346215 = 346215,
			xl346251 = 346251,
			xl346512 = 346512,
			xl346521 = 346521,
			xl351246 = 351246,
			xl351264 = 351264,
			xl351426 = 351426,
			xl351462 = 351462,
			xl351624 = 351624,
			xl351642 = 351642,
			xl352146 = 352146,
			xl352164 = 352164,
			xl352416 = 352416,
			xl352461 = 352461,
			xl352614 = 352614,
			xl352641 = 352641,
			xl354126 = 354126,
			xl354162 = 354162,
			xl354216 = 354216,
			xl354261 = 354261,
			xl354612 = 354612,
			xl354621 = 354621,
			xl356124 = 356124,
			xl356142 = 356142,
			xl356214 = 356214,
			xl356241 = 356241,
			xl356412 = 356412,
			xl356421 = 356421,
			xl361245 = 361245,
			xl361254 = 361254,
			xl361425 = 361425,
			xl361452 = 361452,
			xl361524 = 361524,
			xl361542 = 361542,
			xl362145 = 362145,
			xl362154 = 362154,
			xl362415 = 362415,
			xl362451 = 362451,
			xl362514 = 362514,
			xl362541 = 362541,
			xl364125 = 364125,
			xl364152 = 364152,
			xl364215 = 364215,
			xl364251 = 364251,
			xl364512 = 364512,
			xl364521 = 364521,
			xl365124 = 365124,
			xl365142 = 365142,
			xl365214 = 365214,
			xl365241 = 365241,
			xl365412 = 365412,
			xl365421 = 365421,
			xl412356 = 412356,
			xl412365 = 412365,
			xl412536 = 412536,
			xl412563 = 412563,
			xl412635 = 412635,
			xl412653 = 412653,
			xl413256 = 413256,
			xl413265 = 413265,
			xl413526 = 413526,
			xl413562 = 413562,
			xl413625 = 413625,
			xl413652 = 413652,
			xl415236 = 415236,
			xl415263 = 415263,
			xl415326 = 415326,
			xl415362 = 415362,
			xl415623 = 415623,
			xl415632 = 415632,
			xl416235 = 416235,
			xl416253 = 416253,
			xl416325 = 416325,
			xl416352 = 416352,
			xl416523 = 416523,
			xl416532 = 416532,
			xl421356 = 421356,
			xl421365 = 421365,
			xl421536 = 421536,
			xl421563 = 421563,
			xl421635 = 421635,
			xl421653 = 421653,
			xl423156 = 423156,
			xl423165 = 423165,
			xl423516 = 423516,
			xl423561 = 423561,
			xl423615 = 423615,
			xl423651 = 423651,
			xl425136 = 425136,
			xl425163 = 425163,
			xl425316 = 425316,
			xl425361 = 425361,
			xl425613 = 425613,
			xl425631 = 425631,
			xl426135 = 426135,
			xl426153 = 426153,
			xl426315 = 426315,
			xl426351 = 426351,
			xl426513 = 426513,
			xl426531 = 426531,
			xl431256 = 431256,
			xl431265 = 431265,
			xl431526 = 431526,
			xl431562 = 431562,
			xl431625 = 431625,
			xl431652 = 431652,
			xl432156 = 432156,
			xl432165 = 432165,
			xl432516 = 432516,
			xl432561 = 432561,
			xl432615 = 432615,
			xl432651 = 432651,
			xl435126 = 435126,
			xl435162 = 435162,
			xl435216 = 435216,
			xl435261 = 435261,
			xl435612 = 435612,
			xl435621 = 435621,
			xl436125 = 436125,
			xl436152 = 436152,
			xl436215 = 436215,
			xl436251 = 436251,
			xl436512 = 436512,
			xl436521 = 436521,
			xl451236 = 451236,
			xl451263 = 451263,
			xl451326 = 451326,
			xl451362 = 451362,
			xl451623 = 451623,
			xl451632 = 451632,
			xl452136 = 452136,
			xl452163 = 452163,
			xl452316 = 452316,
			xl452361 = 452361,
			xl452613 = 452613,
			xl452631 = 452631,
			xl453126 = 453126,
			xl453162 = 453162,
			xl453216 = 453216,
			xl453261 = 453261,
			xl453612 = 453612,
			xl453621 = 453621,
			xl456123 = 456123,
			xl456132 = 456132,
			xl456213 = 456213,
			xl456231 = 456231,
			xl456312 = 456312,
			xl456321 = 456321,
			xl461235 = 461235,
			xl461253 = 461253,
			xl461325 = 461325,
			xl461352 = 461352,
			xl461523 = 461523,
			xl461532 = 461532,
			xl462135 = 462135,
			xl462153 = 462153,
			xl462315 = 462315,
			xl462351 = 462351,
			xl462513 = 462513,
			xl462531 = 462531,
			xl463125 = 463125,
			xl463152 = 463152,
			xl463215 = 463215,
			xl463251 = 463251,
			xl463512 = 463512,
			xl463521 = 463521,
			xl465123 = 465123,
			xl465132 = 465132,
			xl465213 = 465213,
			xl465231 = 465231,
			xl465312 = 465312,
			xl465321 = 465321,
			xl512346 = 512346,
			xl512364 = 512364,
			xl512436 = 512436,
			xl512463 = 512463,
			xl512634 = 512634,
			xl512643 = 512643,
			xl513246 = 513246,
			xl513264 = 513264,
			xl513426 = 513426,
			xl513462 = 513462,
			xl513624 = 513624,
			xl513642 = 513642,
			xl514236 = 514236,
			xl514263 = 514263,
			xl514326 = 514326,
			xl514362 = 514362,
			xl514623 = 514623,
			xl514632 = 514632,
			xl516234 = 516234,
			xl516243 = 516243,
			xl516324 = 516324,
			xl516342 = 516342,
			xl516423 = 516423,
			xl516432 = 516432,
			xl521346 = 521346,
			xl521364 = 521364,
			xl521436 = 521436,
			xl521463 = 521463,
			xl521634 = 521634,
			xl521643 = 521643,
			xl523146 = 523146,
			xl523164 = 523164,
			xl523416 = 523416,
			xl523461 = 523461,
			xl523614 = 523614,
			xl523641 = 523641,
			xl524136 = 524136,
			xl524163 = 524163,
			xl524316 = 524316,
			xl524361 = 524361,
			xl524613 = 524613,
			xl524631 = 524631,
			xl526134 = 526134,
			xl526143 = 526143,
			xl526314 = 526314,
			xl526341 = 526341,
			xl526413 = 526413,
			xl526431 = 526431,
			xl531246 = 531246,
			xl531264 = 531264,
			xl531426 = 531426,
			xl531462 = 531462,
			xl531624 = 531624,
			xl531642 = 531642,
			xl532146 = 532146,
			xl532164 = 532164,
			xl532416 = 532416,
			xl532461 = 532461,
			xl532614 = 532614,
			xl532641 = 532641,
			xl534126 = 534126,
			xl534162 = 534162,
			xl534216 = 534216,
			xl534261 = 534261,
			xl534612 = 534612,
			xl534621 = 534621,
			xl536124 = 536124,
			xl536142 = 536142,
			xl536214 = 536214,
			xl536241 = 536241,
			xl536412 = 536412,
			xl536421 = 536421,
			xl541236 = 541236,
			xl541263 = 541263,
			xl541326 = 541326,
			xl541362 = 541362,
			xl541623 = 541623,
			xl541632 = 541632,
			xl542136 = 542136,
			xl542163 = 542163,
			xl542316 = 542316,
			xl542361 = 542361,
			xl542613 = 542613,
			xl542631 = 542631,
			xl543126 = 543126,
			xl543162 = 543162,
			xl543216 = 543216,
			xl543261 = 543261,
			xl543612 = 543612,
			xl543621 = 543621,
			xl546123 = 546123,
			xl546132 = 546132,
			xl546213 = 546213,
			xl546231 = 546231,
			xl546312 = 546312,
			xl546321 = 546321,
			xl561234 = 561234,
			xl561243 = 561243,
			xl561324 = 561324,
			xl561342 = 561342,
			xl561423 = 561423,
			xl561432 = 561432,
			xl562134 = 562134,
			xl562143 = 562143,
			xl562314 = 562314,
			xl562341 = 562341,
			xl562413 = 562413,
			xl562431 = 562431,
			xl563124 = 563124,
			xl563142 = 563142,
			xl563214 = 563214,
			xl563241 = 563241,
			xl563412 = 563412,
			xl563421 = 563421,
			xl564123 = 564123,
			xl564132 = 564132,
			xl564213 = 564213,
			xl564231 = 564231,
			xl564312 = 564312,
			xl564321 = 564321,
			xl612345 = 612345,
			xl612354 = 612354,
			xl612435 = 612435,
			xl612453 = 612453,
			xl612534 = 612534,
			xl612543 = 612543,
			xl613245 = 613245,
			xl613254 = 613254,
			xl613425 = 613425,
			xl613452 = 613452,
			xl613524 = 613524,
			xl613542 = 613542,
			xl614235 = 614235,
			xl614253 = 614253,
			xl614325 = 614325,
			xl614352 = 614352,
			xl614523 = 614523,
			xl614532 = 614532,
			xl615234 = 615234,
			xl615243 = 615243,
			xl615324 = 615324,
			xl615342 = 615342,
			xl615423 = 615423,
			xl615432 = 615432,
			xl621345 = 621345,
			xl621354 = 621354,
			xl621435 = 621435,
			xl621453 = 621453,
			xl621534 = 621534,
			xl621543 = 621543,
			xl623145 = 623145,
			xl623154 = 623154,
			xl623415 = 623415,
			xl623451 = 623451,
			xl623514 = 623514,
			xl623541 = 623541,
			xl624135 = 624135,
			xl624153 = 624153,
			xl624315 = 624315,
			xl624351 = 624351,
			xl624513 = 624513,
			xl624531 = 624531,
			xl625134 = 625134,
			xl625143 = 625143,
			xl625314 = 625314,
			xl625341 = 625341,
			xl625413 = 625413,
			xl625431 = 625431,
			xl631245 = 631245,
			xl631254 = 631254,
			xl631425 = 631425,
			xl631452 = 631452,
			xl631524 = 631524,
			xl631542 = 631542,
			xl632145 = 632145,
			xl632154 = 632154,
			xl632415 = 632415,
			xl632451 = 632451,
			xl632514 = 632514,
			xl632541 = 632541,
			xl634125 = 634125,
			xl634152 = 634152,
			xl634215 = 634215,
			xl634251 = 634251,
			xl634512 = 634512,
			xl634521 = 634521,
			xl635124 = 635124,
			xl635142 = 635142,
			xl635214 = 635214,
			xl635241 = 635241,
			xl635412 = 635412,
			xl635421 = 635421,
			xl641235 = 641235,
			xl641253 = 641253,
			xl641325 = 641325,
			xl641352 = 641352,
			xl641523 = 641523,
			xl641532 = 641532,
			xl642135 = 642135,
			xl642153 = 642153,
			xl642315 = 642315,
			xl642351 = 642351,
			xl642513 = 642513,
			xl642531 = 642531,
			xl643125 = 643125,
			xl643152 = 643152,
			xl643215 = 643215,
			xl643251 = 643251,
			xl643512 = 643512,
			xl643521 = 643521,
			xl645123 = 645123,
			xl645132 = 645132,
			xl645213 = 645213,
			xl645231 = 645231,
			xl645312 = 645312,
			xl645321 = 645321,
			xl651234 = 651234,
			xl651243 = 651243,
			xl651324 = 651324,
			xl651342 = 651342,
			xl651423 = 651423,
			xl651432 = 651432,
			xl652134 = 652134,
			xl652143 = 652143,
			xl652314 = 652314,
			xl652341 = 652341,
			xl652413 = 652413,
			xl652431 = 652431,
			xl653124 = 653124,
			xl653142 = 653142,
			xl653214 = 653214,
			xl653241 = 653241,
			xl653412 = 653412,
			xl653421 = 653421,
			xl654123 = 654123,
			xl654132 = 654132,
			xl654213 = 654213,
			xl654231 = 654231,
			xl654312 = 654312,
			xl654321 = 654321,
			MAX = 654322
		}

		public Material graffitiMaterial;

		public string title;

		public string artistName;

		public GraffitiSize graffitiSize;

		public GraffitiAppEntry unlockable;

		public Combos combos = Combos.NONE;
	}
}
