using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ShopCatalog : UdonSharpBehaviour
{
    public string[] placeNames = new string[] { "Earth", "Astronomy", "Sci-Tech" };

    public string[] names = new string[]
    {
        "Oceans Badge", "Volcanoes and Rocks Badge", "Weather and Climate Badge", "Living Earth Badge", "Places on Earth Badge",
        "Island at Sunset Postcard", "Departures Airport Postcard", "Coral Reef Postcard", "Erupting Volcano Postcard", "Aurora Postcard",
        "Blue Whale Pin", "Lightning Bolt Pin", "Mountain Pin", "Platypus Pin", "Compass Pin",
        "Mini Rocket", "Island Snow Globe", "Beach Ball", "Plush Sea Turtle", "Binoculars",
        "Sun Hat", "Explorer Cap", "Sunglasses", "Flower Crown", "Bucket Hat",
        "Travel Neck Pillow", "Scarf", "Backpack", "Camera on a Strap", "Traveller Lanyard",
        "First Flight Pin", "Orionid Meteor Shower Pin", "Earth Day Seedling", "World Oceans Day Shell", "Earth Explorer Globe",

        "Astronomy Team 1 Badge", "Astronomy Team 2 Badge", "Astronomy Team 3 Badge", "Astronomy Team 4 Badge", "Astronomy Team 5 Badge", "Astronomy Team 6 Badge",
        "Planet from Orbit Postcard", "Spiral Galaxy Postcard", "Ringed Planet Postcard", "Nebula Postcard", "Black Hole Ring Postcard",
        "Telescope Pin", "Comet Pin", "Crescent Moon Pin", "Saturn Pin",
        "Pocket Telescope", "Planet Snow Globe", "Glow Stick", "Plush Astronaut", "Meteorite Fragment",
        "Astronaut Helmet", "Star Beanie", "Red Headlamp", "Orbiting Moons Headband", "Star Visor Glasses",
        "Ringed Planet Halo", "Comet Tail Scarf", "Jetpack", "Constellation Cape", "Star Map Lanyard",
        "First Flight Astronomy Patch", "Leonid Meteor Shower Pin", "Geminid Meteor Shower Pin", "Great Eclipse 2027 Pin", "Astronomy Collector Telescope"
    };

    public int[] kinds = new int[]
    {
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        1, 1, 1, 1, 1,
        2, 2, 2, 2, 2,
        2, 2, 2, 2, 2,
        0, 0, 0, 0, 0,

        0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0,
        1, 1, 1, 1, 1,
        2, 2, 2, 2, 2,
        2, 2, 2, 2, 2,
        0, 0, 0, 0, 0
    };

    public int[] prices = new int[]
    {
        15, 15, 15, 15, 15,
        5, 5, 5, 5, 5,
        8, 8, 8, 8, 8,
        10, 12, 6, 10, 8,
        10, 10, 8, 10, 8,
        8, 8, 12, 10, 6,
        25, 25, 25, 25, 30,

        20, 20, 20, 20, 20, 20,
        5, 5, 5, 5, 5,
        8, 8, 8, 8,
        12, 12, 3, 10, 10,
        15, 8, 8, 12, 8,
        12, 10, 15, 12, 6,
        25, 25, 25, 25, 30
    };

    public int[] limitType = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        1, 2, 2, 2, 3,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        1, 2, 2, 2, 3
    };

    public int[] startDate = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 20261015, 20270415, 20270601, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 20261110, 20261207, 20270726, 0
    };

    public int[] endDate = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        20261106, 20261031, 20270429, 20270614, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        20261106, 20261124, 20261221, 20270809, 0
    };

    public int[] needFrom = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 35
    };

    public int[] needTo = new int[]
    {
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 4,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 40
    };

    [TextArea(2, 3)]
    public string[] info = new string[]
    {
        "A badge for the Oceans quiz booth.",
        "A badge for the Volcanoes and Rocks quiz booth.",
        "A badge for the Weather and Climate quiz booth.",
        "A badge for the Living Earth quiz booth.",
        "A badge for the Places on Earth quiz booth.",
        "The island at golden hour.",
        "Rockets waiting at the Departures gates.",
        "A bright coral reef full of fish.",
        "A volcano lighting up the night.",
        "Green auroras dancing over the snow.",
        "The largest animal alive today.",
        "Lightning can be about five times hotter than the Sun's surface.",
        "For everyone who loves a view from the top.",
        "One of the only mammals that lays eggs.",
        "Never get lost on the island.",
        "A small rocket to fly around by hand.",
        "The island inside a dome. Give it a shake.",
        "A bright ball for the beach.",
        "A soft little sea turtle.",
        "Spot ships and birds from the lookout.",
        "A wide straw hat for sunny days.",
        "A cap for island explorers.",
        "Cool shades for the beach.",
        "A crown of bright tropical flowers.",
        "A soft bucket hat for hiking.",
        "Every traveller needs one for the flight.",
        "A light scarf in ocean blue.",
        "A small backpack for your adventures.",
        "Hang it round your neck and pose for photos.",
        "A lanyard that says Centaurus Traveller.",
        "LIMITED: only sold until our first big demo on 6 November 2026.",
        "LIMITED: only sold during the Orionid meteor shower, 15 to 31 October.",
        "LIMITED: only sold around Earth Day, 15 to 29 April.",
        "LIMITED: only sold around World Oceans Day, 1 to 14 June.",
        "LIMITED: unlocks when you own all five Earth booth badges.",

        "A badge for a research team from the Astronomy activities.",
        "A badge for a research team from the Astronomy activities.",
        "A badge for a research team from the Astronomy activities.",
        "A badge for a research team from the Astronomy activities.",
        "A badge for a research team from the Astronomy activities.",
        "A badge for a research team from the Astronomy activities.",
        "The Astronomy planet seen from orbit.",
        "A spiral galaxy with glowing arms.",
        "A planet with bright rings.",
        "A colourful cloud where stars are born.",
        "A glowing ring around a black hole.",
        "For every stargazer.",
        "A comet with a long tail.",
        "A thin crescent Moon.",
        "The ringed planet.",
        "A tiny telescope to hold up to your eye.",
        "The Astronomy planet inside a dome.",
        "A glowing stick in planet colours.",
        "A soft little astronaut.",
        "A dark rock that fell from space.",
        "A white helmet with a gold visor.",
        "A navy beanie covered in tiny stars.",
        "Astronomers use red light because it keeps their eyes used to the dark.",
        "Two little moons circle your head.",
        "Glowing visor glasses for stargazing.",
        "A ring that floats above your head like a tiny planet.",
        "A scarf with a glowing comet tail.",
        "A jetpack for exploring the planet.",
        "A dark cape with the constellations on it.",
        "A lanyard with a mini star map.",
        "LIMITED: only sold until our first big demo on 6 November 2026.",
        "LIMITED: only sold during the Leonid meteor shower, 10 to 24 November.",
        "LIMITED: only sold during the Geminid meteor shower, 7 to 21 December.",
        "LIMITED: only sold around the total solar eclipse of 2 August 2027.",
        "LIMITED: unlocks when you own all six Astronomy team badges."
    };

    public int GetCount()
    {
        return names.Length;
    }

    public int Today()
    {
        System.DateTime now = System.DateTime.UtcNow;
        return now.Year * 10000 + now.Month * 100 + now.Day;
    }

    public bool IsOnSale(int id, CoinWallet wallet)
    {
        int type = limitType[id];
        int today = Today();

        if (type == 1) return today <= endDate[id];
        if (type == 2) return today >= startDate[id] && today <= endDate[id];
        if (type == 3)
        {
            for (int i = needFrom[id]; i <= needTo[id]; i++)
            {
                if (!wallet.HasItem(i)) return false;
            }
        }
        return true;
    }
}
