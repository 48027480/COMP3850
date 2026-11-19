using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Archaeology3_ContextCluesBank : UdonSharpBehaviour
{
    public string[] artifactNames = new string[]
    {
        "The Antikythera Mechanism",
        "The Nazca Lines",
        "The Moai",
        "Stonehenge",
        "Lascaux Cave"
    };

    [TextArea(2, 4)]
    public string[] findText = new string[]
    {
        "A lump of corroded bronze about the size of a shoebox. Inside are tiny hand-cut gear wheels, and Greek letters are engraved on its plates. It is crusted with sea growth.",
        "A broken clay pot painted in many colours with a hummingbird and a monkey. It was picked up beside a long, dead straight line where dark stones had been raked aside.",
        "A chunk of soft grey-brown volcanic rock, carved into part of a long ear and a heavy brow. A piece of white coral, shaped like an eye, was found with it.",
        "A chip of blue-grey stone with pale speckles, knocked off a huge standing stone. It was found next to a digging pick made from a red deer antler.",
        "A small, hollowed stone lamp with old soot inside, a lump of red and black mineral pigment, and a tool carved from reindeer antler."
    };

    public string[] standNames = new string[]
    {
        "Knossos, Crete",
        "A Viking ship burial, Norway",
        "A shipwreck off Antikythera, Greece",
        "A clockmaker's workshop, Italy",

        "Uffington White Horse, England",
        "The desert near Nazca, Peru",
        "Serpent Mound, Ohio, USA",
        "Murujuga, Western Australia",

        "Olmec heartland, Mexico",
        "Tiwanaku, Bolivia",
        "A Maori meeting house, New Zealand",
        "Rano Raraku, Rapa Nui (Easter Island)",

        "Salisbury Plain, England",
        "Carnac, France",
        "Newgrange, Ireland",
        "Gobekli Tepe, Turkey",

        "Ajanta Caves, India",
        "Ubirr rock shelters, Australia",
        "Lascaux, southwestern France",
        "Chauvet Cave, France"
    };

    [TextArea(2, 4)]
    public string[] surfaceClues = new string[]
    {
        "A huge Bronze Age palace inland on Crete, with painted walls and storerooms full of tall clay jars.",
        "A wooden ship buried under a mound of earth, with sledges, a cart and fine textiles.",
        "A cargo ship wreck about 45 to 55 m down off a small Greek island, full of bronze and marble statues and wine jars.",
        "A town workshop full of iron tools, brass offcuts and early clock parts.",

        "A huge stylised horse cut into a chalk hillside, about 110 m long.",
        "A dry, flat, stony plain where dark surface stones were raked aside to show paler ground below.",
        "A long earth bank shaped like a snake, on a ridge above a creek.",
        "Rocky ridges by the sea, covered in carved animal and human figures.",

        "Huge carved stone heads in a hot, wet lowland near the coast.",
        "Stone gateways and statues on a high plateau near Lake Titicaca.",
        "A meeting house with carved posts and panels showing ancestors.",
        "The slopes of an old volcanic crater on a remote Pacific island, dotted with half-carved stone figures.",

        "Open chalk grassland with a ring of huge standing stones inside a circular ditch.",
        "Thousands of standing stones set in long rows near the coast.",
        "A great round passage tomb with spirals carved on its entrance stone.",
        "Carved T-shaped stone pillars on a hilltop.",

        "Halls cut into a cliff, with painted walls and ceilings.",
        "Rock shelters painted with fish, turtles and people.",
        "A cave in a wooded hillside, found in 1940 by four teenagers.",
        "A cave with paintings of lions, rhinos and horses, found in 1994."
    };

    [TextArea(2, 4)]
    public string[] labClues = new string[]
    {
        "Lab result: the palace was at its height about 1900 to 1400 BCE, centuries before Greek letters like the ones on the find were used.",
        "Lab result: tree rings date the ship's timber to the 800s CE, and no Greek writing has ever been found in the burial.",
        "Lab result: coins and pottery from the wreck date it to the 1st century BCE. The sea growth and the corrosion on the find match objects from this wreck.",
        "Lab result: the clock parts date to the 1300s CE. A crust like the one on the find takes many centuries on the sea floor.",

        "Lab result: soil dating puts the horse at about 3,000 years old. Hummingbirds only live in the Americas, so the pot's painting cannot come from here.",
        "Lab result: broken pots along the lines are painted in the Nazca style, which dates to roughly 2,200 to 1,300 years ago. The pot matches them.",
        "Lab result: the snake is piled up from soil, not cleared from stony desert, and no painted pots like this one have been found here.",
        "Lab result: these figures are pecked into hard rock, not cleared from a desert floor. Hummingbirds only live in the Americas, so the pot's painting cannot come from here.",

        "Lab result: the Olmec heads are basalt, a hard dark rock, and they have no long ears or coral eyes.",
        "Lab result: Tiwanaku's statues are andesite and sandstone, and their style has no long ears or coral eyes.",
        "Lab result: these carvings are wood. The find is volcanic rock.",
        "Lab result: the find's rock is tuff from this crater, which almost every statue on the island was carved from. Coral eyes were set into some of them.",

        "Lab result: antler picks from the ditch date to about 3000 BCE. The speckled stone is spotted dolerite, like the 'bluestones' here, which came from the Preseli Hills in Wales, more than 200 km away.",
        "Lab result: the Carnac stones are local granite. There is no spotted dolerite from Wales here.",
        "Lab result: Newgrange is built mostly of greywacke, with white quartz brought from the Wicklow Mountains. There is no spotted dolerite.",
        "Lab result: these pillars are limestone and date to more than 11,000 years ago, far older than the antler pick.",

        "Lab result: these paintings are about 2,200 to 1,500 years old, and reindeer never lived in India.",
        "Lab result: reindeer never lived in Australia, so a reindeer antler tool cannot come from here.",
        "Lab result: soot and charcoal from this cave date to around 21,000 years ago, when reindeer lived in the area. A carved stone lamp was also found inside.",
        "Lab result: charcoal from these paintings dates to around 36,000 years ago, far older than the lamp's soot."
    };

    public int[] correctStand = new int[] { 2, 1, 3, 0, 2 };

    public string[] hint1 = new string[]
    {
        "Not quite. Which place could leave a sea crust on bronze?",
        "Not quite. Which place has birds like the one painted on the pot?",
        "Not quite. What kind of rock is the find made of?",
        "Not quite. Where could the speckled stone have come from?",
        "Not quite. Where and when did reindeer live?"
    };

    public string[] hint2 = new string[]
    {
        "Read the lab results again. Only one place has Greek writing and centuries under the sea.",
        "Read the lab results again. Only one place has painted pots that match.",
        "Read the lab results again. Only one place has statues carved from volcanic tuff.",
        "Read the lab results again. Only one place has spotted dolerite and antler picks.",
        "Read the lab results again. Only one date matches the lamp's soot."
    };

    public string[] facts = new string[]
    {
        "Recovered in 1900 to 1901 from a Roman era shipwreck near the Greek island of Antikythera. Often called an ancient analog computer, used to track astronomical cycles.",
        "Giant shapes scratched into the desert floor of southern Peru, some stretching hundreds of metres, best seen from the air.",
        "The large stone statues of Rapa Nui (Easter Island), built between the 10th and 16th centuries.",
        "A stone circle in southern England, with its earliest phases dating back roughly 5,000 years.",
        "A cave in southwestern France found in 1940 by four teenagers, holding wall paintings made by people who used the cave about 21,000 years ago."
    };

    public int GetCount()
    {
        return artifactNames.Length;
    }
}
