using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class ComparePuzzleBank : UdonSharpBehaviour
{
    [TextArea(2, 4)]
    public string[] questions = new string[]
    {
        "Out of the two black holes that the Event Horizon Telescope imaged, which one's ring looks bigger from Earth?",
        "Out of the two objects from beyond our Solar System that passed the Sun, which one behaved like an ordinary comet?",
        "In 2025, two studies looked at the planet K2-18b for a possible sign of life. Which conclusion does the evidence support right now?",
        "What best explains Betelgeuse's cycle of brightening and dimming that lasts around six years?",
        "What is the best current explanation for compact 'little red dots' in the early universe that JWST keeps finding?"
    };

    public string[] labelA = new string[]
    {
        "Sgr A*",
        "1I/'Oumuamua",
        "Study A: Joint reanalysis",
        "Idea A: Supernova soon",
        "Idea A: Black hole star"
    };

    [TextArea(3, 6)]
    public string[] infoA = new string[]
    {
        "The black hole at the centre of our Milky Way.\nMass: about 4 million Suns.\nDistance: about 27,000 light-years.\nGas orbits it in minutes.",
        "It was found in 2017.\nAs it left the Sun it sped up slightly, as if pushed by escaping gas.\nDeep images found no dust, no coma and no tail.",
        "Combined all the JWST data on K2-18b, from 0.6 to 12 microns.\nInsufficient evidence was found for the molecules DMS or DMDS.\nAnother team showed chemistry driven by sunlight which can make DMS without life.",
        "Betelgeuse is a red supergiant and will explode one day.\nIts big 2019 to 2020 dimming caused headlines about an explosion.\nStudies have found that dimming was not a sign of a supernova coming soon.",
        "A growing black hole wrapped in a thick cocoon of gas and not dust.\nIn 2026, Nature published MoM-BH*-1, which gives off almost pure light of this kind."
    };

    public string[] labelB = new string[]
    {
        "M87*",
        "3I/ATLAS",
        "Study B: MIRI result",
        "Idea B: Hidden companion",
        "Idea B: Dusty packed galaxies"
    };

    [TextArea(3, 6)]
    public string[] infoB = new string[]
    {
        "The black hole at the centre of the galaxy M87.\nMass: about 6.5 billion Suns.\nDistance: about 55 million light-years.\nGas takes days to weeks to orbit it.",
        "It was found on 1 July 2025 by the ATLAS survey in Chile.\nIt is moving at about 58 km/s, faster than any object before it.\nHubble saw a dust plume on its sunward side and a hint of a tail.",
        "Used one JWST instrument, MIRI, on its own.\nReported a signal matching DMS or DMDS at 3 sigma.\nOn Earth, DMS is made mainly by marine microbes.",
        "In 2025, Gemini North imaged a faint star inside Betelgeuse's outer atmosphere.\nIn 2026, nearly eight years of Hubble data showed a dense trail of gas behind it.\nThe trail shows up about every six years.",
        "Very compact galaxies packed with stars, reddened by dust.\nThis was one of the first ideas when the dots were found.\nIt needs huge numbers of stars squeezed into a very small space."
    };

    [TextArea(3, 6)]
    public string[] briefs = new string[]
    {
        "A black hole's real size grows in step with its mass. 1,000 times the mass means 1,000 times the size.\n\nHow big it looks from Earth is its size divided by its distance.",
        "An ordinary comet warms up near the Sun and releases gas and dust.\n\nThat material forms a fuzzy coma and a tail we can photograph.\n\nSpeed alone does not tell you what an object is.",
        "In physics, a discovery usually needs 5 sigma, not 3.\n\nA result also has to hold up when other teams test it using all the available data.",
        "The 2019 to 2020 Great Dimming was a one-off, caused by a cloud of dust.\n\nThis question is about a cycle that keeps repeating.\n\nThink about what kind of cause repeats on a schedule.",
        "Normal growing black holes shine brightly in X-rays.\n\nStacked Chandra data show little red dots are far weaker in X-rays than that.\n\nDust can make light look red, but so can thick gas, and thick gas can also hide X-rays."
    };

    public string[] reasons = new string[]
    {
        "It is more massive, so its ring is bigger.",
        "It's much closer to Earth, which more than makes up for its smaller size.",
        "Because the gas moves faster it spreads the glow wider.",

        "It sped up as it moved away from the Sun.",
        "It was moving faster than any object before it.",
        "A telescope saw dust coming off it, forming a coma and a tail.",

        "It tested all the data together, but the signal was not strong enough and did not hold up.",
        "It used a newer telescope than the other study.",
        "It proved that K2-18b has no ocean.",

        "Dimming always comes right before a supernova.",
        "Something orbiting the star comes back on a regular schedule.",
        "The 2019 dust cloud explains every dip since then.",

        "Dust always blocks every X-ray completely.",
        "Galaxies full of stars give off strong X-rays.",
        "Thick gas can explain both the red colour and the weak X-rays."
    };

    public int[] correctObject = new int[] { 0, 1, 0, 1, 0 };
    public int[] correctReason = new int[] { 1, 2, 0, 1, 2 };

    [TextArea(2, 4)]
    public string[] hint1 = new string[]
    {
        "Work out how many times heavier one is, and how many times farther away it is.",
        "Read the briefing card again. What do we normally see around a comet?",
        "Did both studies use the same telescope? Check before you pick a reason.",
        "Read the briefing card. Is the Great Dimming the thing you are explaining?",
        "Use the X-ray clue on the briefing card. What could hide a black hole's X-rays?"
    };

    [TextArea(2, 4)]
    public string[] hint2 = new string[]
    {
        "M87* is about 1,600 times heavier, but about 2,000 times farther away. Which one wins?",
        "Speeding up could be escaping gas, but only one object showed visible dust.",
        "Which study passes the test on the briefing card: all the data, checked by other teams?",
        "A one-off event can't explain a repeating cycle. What comes back every six years?",
        "Which idea already has thick gas in it?"
    };

    [TextArea(3, 6)]
    public string[] correctText = new string[]
    {
        "Correct. Sgr A*'s ring measured about 52 microarcseconds across, against about 42 for M87*. M87* is about 1,600 times heavier but about 2,000 times farther away. (EHT Collaboration 2019, 2022)",
        "Correct. Hubble saw dust coming off 3I/ATLAS (Jewitt et al. 2025). 'Oumuamua did speed up as if outgassing, but no coma, dust or tail was ever seen, and that is still debated (Micheli et al. 2018).",
        "Correct. A joint analysis of all the JWST data found insufficient evidence for DMS or DMDS (Luque et al. 2025). The 3 sigma claim (Madhusudhan et al. 2025) is still debated, and more JWST data is coming.",
        "Correct. A companion star, Siwarha, orbits inside Betelgeuse's outer atmosphere and leaves a wake seen about every six years (Howell et al. 2025; Dupree et al. 2026). The Great Dimming was a separate dust event (Montarges et al. 2021).",
        "Correct, as far as current evidence goes. Black hole stars are favoured for at least some little red dots (de Graaff et al. 2025; Naidu et al. 2026). Whether all of them work this way is still being argued."
    };

    [TextArea(3, 6)]
    public string[] revealText = new string[]
    {
        "The answer is Sgr A*, because it is far closer. M87* is about 1,600 times heavier but about 2,000 times farther away, so Sgr A*'s ring looks slightly bigger: about 52 against 42 microarcseconds.",
        "The answer is 3I/ATLAS. Hubble saw a dust plume and a hint of a tail. 'Oumuamua sped up as if pushed by gas, but no dust or tail was ever seen.",
        "The answer is Study A, the joint reanalysis. When all the JWST data was tested together, the DMS signal did not hold up. Both studies used JWST, and neither showed whether K2-18b has an ocean.",
        "The answer is the hidden companion, Siwarha. Something that orbits comes back on a schedule, which fits a six-year cycle. The Great Dimming was a separate, one-off dust cloud.",
        "The answer is the black hole star. Thick gas around a growing black hole can make the light red and hide its X-rays. It is the current favourite, but the debate is still open."
    };

    void Start()
    {
        if (reasons.Length != questions.Length * 3)
            Debug.LogWarning("Compare bank: reasons needs 3 per question");
    }

    public int GetCount()
    {
        return questions.Length;
    }

    public string GetReason(int p, int i)
    {
        int index = p * 3 + i;
        if (index < 0 || index >= reasons.Length) return "";
        return reasons[index];
    }
}