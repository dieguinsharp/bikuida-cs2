using System;
using System.Collections.Generic;
using System.Text;

namespace Bikuida.Helpers
{
    public class Cs2WeaponsHelpers
    {
        public string GetWeaponByID(int id)
        {
            return id switch
            {
                1 => "Desert Eagle",
                2 => "Dual Berettas",
                3 => "Five-SeveN",
                4 => "Glock",
                7 => "AK-47",
                8 => "AUG",
                9 => "AWP",
                10 => "FAMAS",
                11 => "G3SG1",
                13 => "Galil AR",
                14 => "M249",
                16 => "M4A4",
                17 => "MAC-10",
                19 => "P90",
                24 => "UMP-45",
                25 => "XM1014",
                26 => "PP-Bizon",
                27 => "MAG-7",
                28 => "Negev",
                29 => "Sawed-Off",
                30 => "Tec-9",
                31 => "Zeus x27",
                32 => "P2000",
                33 => "MP7",
                34 => "MP9",
                35 => "Nova",
                36 => "P250",
                38 => "SCAR-20",
                39 => "SG 553",
                40 => "SSG 08",
                41 => "FACA1",
                42 => "FACA2",
                43 => "Galil",
                44 => "Granada",
                45 => "Smoke",
                46 => "Molotov",
                47 => "Decoy",
                48 => "Incgrenage",
                49 => "C4",
                59 => "FACA",
                60 => "M4A1-S",
                61 => "USP-S",
                63 => "CZ75-Auto",
                64 => "R8",
                80 => "FACA",
                500 => "Knife Bayonet",
                505 => "Knife Flip",
                506 => "Knife Gut",
                507 => "Knife Karambit",
                508 => "Knife M9",
                509 => "Knife Tactica",
                512 => "Knife Falchion",
                514 => "Knife Survival Bowie",
                515 => "Knife Butterfly",
                516 => "Knife Rush",
                519 => "Knife Ursus",
                520 => "Knife Gypsy Jackknife",
                522 => "Knife Stiletto",
                523 => "Knife Widowmaker",
                _ => "Unknown Weapon"
            };
        }
    }
}
