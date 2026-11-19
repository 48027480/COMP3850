using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech4_FixBank : UdonSharpBehaviour
{
    string[] titles = new string[]
    {
        "Apollo 13: the square peg, 1970",
        "Apollo 17: the broken fender, 1972",
        "San Jose Mine: the first paloma, 2010",
        "Tham Luang Cave: the dive kit, 2018",
        "Space Station: the torn solar wing, 2007"
    };

    string[] briefs = new string[]
    {
        "An oxygen tank has exploded. The crew is living in the lunar module, and carbon dioxide is building up. The command module's spare filters are square; the lunar module takes round ones. Build an adapter so a square filter can clean the lunar module's air, using only what is on board.",
        "On the Moon, a rear fender extension has broken off the lunar rover. Without it, the wheel throws up rooster tails of dust over the crew and their gear. Tape on its own won't hold in the dust. Build a new fender extension from things the crew no longer needs.",
        "Thirty-three miners have been trapped about 700 m down for 17 days, eating tiny rations. A narrow borehole now reaches them, and a long steel tube called a paloma can be lowered through it. Pack the first paloma. Think about what bodies that have barely eaten can handle.",
        "Twelve boys and their coach are on a ledge deep in a flooded cave. None of them can swim, and the way out includes long underwater sections in near-zero visibility. Get one boy ready to be taken out by a rescue diver.",
        "A solar wing on the space station tore while it was being extended, leaving a rip of about 2.5 feet. The wing is generating electricity. Fix it so the wing can be fully extended, without putting the astronaut at risk of a shock."
    };

    int[] slotCounts = new int[] { 5, 4, 3, 4, 5 };
    bool[] anyOrder = new bool[] { false, false, true, false, false };

    string[] slotLabels = new string[]
    {
        "Base", "Arch over the top", "Airtight cover", "Air inlet", "Seal",
        "Fender panel", "Joins", "Clamp A", "Clamp B", "",
        "Compartment 1", "Compartment 2", "Compartment 3", "", "",
        "Face", "Chest", "Body", "Back", "",
        "Snagged wire", "Tear point A", "Tear point B", "Tear point C", "Keep-away tool"
    };

    int[] slotAnswers = new int[]
    {
        0, 2, 1, 3, 4,
        0, 1, 2, 2, -1,
        0, 1, 2, -1, -1,
        0, 1, 2, 3, -1,
        0, 1, 1, 1, 2
    };

    string[] partNames = new string[]
    {
        "Square lithium hydroxide canister", "Plastic bag", "Flight plan cover card", "Spacesuit hose", "Grey duct tape", "Flashlight", "Food packet", "Pencil",
        "Four laminated lunar maps", "Grey duct tape", "Lamp clamp", "Geology hammer", "Sample bag", "Rake", "", "",
        "Glucose gel", "Rehydration tablets", "Water", "Canned tuna", "Bread rolls", "Hot cooked meal", "", "",
        "Full-face mask", "Air cylinder", "5 mm wetsuit", "Harness with back handle", "Standard mask and mouthpiece", "Swim fins", "Life jacket", "",
        "Kapton-taped wire cutters", "Cufflink", "Insulated hockey stick", "Bare metal pliers", "Spare solar panel", "", "", ""
    };

    string[] hint1 = new string[]
    {
        "Every real part came from the spacecraft. Three things here are just clutter.",
        "The maps were finished with. What would make them stay put in the dust?",
        "After 17 days on almost nothing, a heavy meal could make them ill.",
        "None of the boys could swim or had ever dived. What is easiest to breathe from?",
        "The wing is live. Anything bare metal near it is a risk."
    };

    string[] hint2 = new string[]
    {
        "The card makes an arch, the bag goes over everything, and the hose goes into the bag.",
        "The maps were taped together, then held on with two clamps borrowed from the cabin lights.",
        "Think recovery first: sugar, salts and fluid.",
        "The cylinder went on the boy's front, and the diver held a handle on his back.",
        "Cut the snag, brace the tear in several places, and keep a non-conducting tool between you and the wing."
    };

    string[] correctTexts = new string[]
    {
        "Fixed. Mission Control worked this \"mailbox\" out on the ground and talked the crew through building it over the radio.",
        "Fixed. Cernan taped four maps together inside the lunar module, where the tape stayed sticky, and clamped it onto the rover.",
        "Fixed. The first palomas carried glucose gel and rehydration tablets for starved digestive systems.",
        "Fixed. Each boy wore a full-face mask and wetsuit, with a cylinder strapped to his front, and a diver guided him along the line by a handle on his back.",
        "Fixed. Parazynski cut the snagged wire and fitted five cufflinks, and the crew then extended the wing fully."
    };

    string[] revealTexts = new string[]
    {
        "The real fix: canister, card arch, plastic bag, suit hose and grey duct tape.",
        "The real fix: four laminated maps taped together, held on by two lamp clamps.",
        "The real fix: glucose gel, rehydration tablets and water came first. Solid food came later.",
        "The real fix: full-face mask, air cylinder on the front, 5 mm wetsuit and a harness with a handle on the back.",
        "The real fix: insulated cutters for the snag, cufflinks across the tear and an insulated hockey stick to keep clear."
    };

    string[] stories = new string[]
    {
        "Apollo 13, April 1970. Lovell, Swigert and Haise were in Aquarius, the lunar module, after an oxygen tank exploded. The crew built the adapter from a lithium hydroxide canister, a plastic bag, a flight plan cover, a spacesuit hose and grey tape. The original went down with Aquarius; a replica is at the Smithsonian.",
        "Apollo 17, December 1972. The rover's right rear fender extension broke off, and a first fix with tape alone failed in the dust. Overnight, John Young and a team in Houston worked out the fix: four of the 28 lunar maps, duct tape, and two clamps from the optical alignment telescope lamp.",
        "San Jose mine, Chile, 2010. On day 17 a drill broke through with a note attached: all 33 were alive. The 1.5 m palomas then carried gel, tablets, water, medicine and letters. All 33 were winched up in the Fenix capsule on 12 and 13 October, after 69 days underground.",
        "Tham Luang, Thailand, July 2018. British divers found the Wild Boars team on 2 July. Over three days, divers brought all 13 out through about 3 km of cave along guide lines, with spare air tanks staged on the way. The boys were also given medication to keep them calm.",
        "STS-120, 3 November 2007. The crew made the cufflinks from strips of aluminium, a hole punch, a bolt connector and about 66 feet of wire. Parazynski rode on the end of the robotic arm and the boom, with his tools wrapped in Kapton tape against electrical current."
    };

    public int Count() { return titles.Length; }
    public string GetTitle(int r) { return titles[r]; }
    public string GetBrief(int r) { return briefs[r]; }
    public int GetSlotCount(int r) { return slotCounts[r]; }
    public bool IsAnyOrder(int r) { return anyOrder[r]; }
    public string GetSlotLabel(int r, int s) { return slotLabels[r * 5 + s]; }
    public int GetSlotAnswer(int r, int s) { return slotAnswers[r * 5 + s]; }
    public string GetPartName(int r, int t) { return partNames[r * 8 + t]; }
    public string GetHint1(int r) { return hint1[r]; }
    public string GetHint2(int r) { return hint2[r]; }
    public string GetCorrectText(int r) { return correctTexts[r]; }
    public string GetRevealText(int r) { return revealTexts[r]; }
    public string GetStory(int r) { return stories[r]; }
}
