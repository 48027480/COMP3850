using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class IdentifyPuzzleBank : UdonSharpBehaviour
{
    [TextArea(2, 4)]
    public string[] questions = new string[]
    {
        "In June 2023, several teams announced evidence for a low hum of gravitational waves passing through our galaxy. Which kind of experiment found it?",
        "In 2024, astronomers found a young ocean hidden under the ice of a small moon, just from the way the moon moves in its orbit. Which moon?",
        "In June 2025, a new observatory released its first images and found over 2,000 new asteroids in about 10 hours of test observing. Which one?",
        "One measurement of how fast the universe is expanding gives about 73 km/s per megaparsec, well above what the early universe predicts. Which measurement?",
        "On 24 December 2024, a spacecraft passed about 6.1 million km from the Sun's surface, moving faster than any human-made object ever has. Which spacecraft?"
    };

    public string[] names = new string[]
    {
        "LIGO-Virgo-KAGRA", "Pulsar timing array", "LISA", "BICEP/Keck",
        "Mimas", "Enceladus", "Titan", "Europa",
        "Rubin Observatory", "Extremely Large Telescope", "Roman Space Telescope", "ALMA",
        "Planck", "SH0ES", "DESI", "CCHP",
        "Parker Solar Probe", "Solar Orbiter", "BepiColombo", "Helios 2"
    };

    [TextArea(2, 4)]
    public string[] cases = new string[]
    {
        "Laser detectors on the ground with arms kilometres long. They heard the first gravitational waves in 2015.",
        "Radio telescopes timing dozens of fast-spinning pulsars, again and again, over many years.",
        "Three spacecraft planned to fly about 2.5 million km apart in a triangle.",
        "Telescopes at the South Pole mapping polarisation in the cosmic microwave background.",

        "A small, icy moon of Saturn covered in craters.",
        "A small, bright, icy moon of Saturn.",
        "Saturn's largest moon.",
        "An icy moon of Jupiter with a cracked surface.",

        "A survey telescope on Cerro Pachon, Chile, with the largest camera ever built.",
        "ESO's giant telescope being built on Cerro Armazones, Chile. Its main mirror is 39 metres wide.",
        "NASA's infrared telescope in space, with a 2.4 metre mirror.",
        "An array of radio dishes on the Chajnantor plateau, Chile, observing millimetre waves.",

        "A space telescope that mapped the cosmic microwave background, the oldest light in the universe.",
        "A team using the Hubble Space Telescope to measure distances to nearby galaxies, step by step.",
        "A survey mapping more than 14 million galaxies and quasars to measure a pattern in how they cluster.",
        "A team using Hubble and JWST to measure distances step by step, with red giant stars as a key step.",

        "A NASA spacecraft that first broke the record for closest approach to the Sun in 2018.",
        "An ESA-led mission with NASA, launched in February 2020 to study the Sun.",
        "A joint ESA and JAXA mission to Mercury.",
        "A German-American probe that set the record for closest approach to the Sun in April 1976."
    };

    [TextArea(2, 4)]
    public string[] clueA = new string[]
    {
        "These waves take years to pass. That is far too slow for detectors on the ground.",
        "The moon orbits Saturn.",
        "It is on the ground, not in space.",
        "It measures nearby galaxies, not the oldest light in the universe.",
        "It was launched this century."
    };

    [TextArea(2, 4)]
    public string[] clueB = new string[]
    {
        "The result used well over ten years of data from an experiment that is already running.",
        "Its surface shows no geysers or plumes, only old craters.",
        "It sees visible light, not radio or millimetre waves.",
        "It does not use the pattern of how galaxies cluster.",
        "Its main target is the Sun, not a planet."
    };

    [TextArea(2, 4)]
    public string[] clueC = new string[]
    {
        "It showed up as tiny timing shifts that line up between pairs of objects across the sky.",
        "It has no thick atmosphere.",
        "Its main mirror is 8.4 metres wide.",
        "Its distance steps rely on pulsating Cepheid stars.",
        "It is a NASA mission, not one led by ESA."
    };

    public int[] correctPad = new int[] { 1, 0, 0, 1, 0 };

    [TextArea(2, 4)]
    public string[] hint1 = new string[]
    {
        "Each clue rules out one pad. Which one does yours remove?",
        "Each clue rules out one moon. Share yours and cross them off.",
        "Each clue rules out one observatory. Which one does yours remove?",
        "Each clue rules out one team. Cross them off together.",
        "Each clue rules out one spacecraft. Which one does yours remove?"
    };

    [TextArea(2, 4)]
    public string[] hint2 = new string[]
    {
        "After all three clues, only one is left: the one that uses very regular cosmic clocks.",
        "Which Saturn moon has geysers, and which has a thick atmosphere?",
        "Roman is in space, ALMA sees millimetre waves, and the ELT's mirror is 39 metres.",
        "Planck sees the oldest light, DESI uses galaxy clustering, and CCHP leans on red giants.",
        "Helios 2 flew in the 1970s, BepiColombo is a Mercury mission, and Solar Orbiter is ESA-led."
    };

    [TextArea(3, 6)]
    public string[] correctText = new string[]
    {
        "Correct. Pulsar timing arrays, including NANOGrav (67 pulsars, 15 years) and Australia's Parkes Pulsar Timing Array (30 pulsars, 18 years), reported evidence in June 2023. It is strong evidence, not yet a 5 sigma detection.",
        "Correct. Mimas has a global ocean 20 to 30 km below its cratered surface, likely less than 25 million years old, found from Cassini tracking of its orbit (Lainey et al. 2024). It is so young that no signs have reached the surface yet.",
        "Correct. Rubin released its First Look images on 23 June 2025. In just over 10 hours of test observing it found 2,104 new asteroids, including 7 near-Earth asteroids.",
        "Correct. SH0ES measured 73.04 plus or minus 1.04 km/s/Mpc using Cepheids in the hosts of 42 Type Ia supernovae, about 5 sigma above Planck's prediction (Riess et al. 2022). The CCHP team, using red giants, gets about 70, so the debate is live.",
        "Correct. Parker Solar Probe passed about 6.1 million km (3.8 million miles) from the Sun's surface at about 692,000 km/h (430,000 mph), faster than any human-made object has moved (NASA)."
    };

    [TextArea(3, 6)]
    public string[] revealText = new string[]
    {
        "The answer is the pulsar timing array. Ground detectors hear waves lasting fractions of a second, LISA has not launched yet, and BICEP/Keck maps the microwave sky.",
        "The answer is Mimas. Europa orbits Jupiter, Enceladus has geysers, and Titan has a thick atmosphere. Mimas's ocean was found from its orbit, not its surface.",
        "The answer is the Rubin Observatory. Roman is in space, ALMA observes millimetre waves, and the ELT's mirror is 39 metres wide and still being built.",
        "The answer is SH0ES. Planck uses the oldest light, DESI uses galaxy clustering, and CCHP uses red giant stars instead of Cepheids.",
        "The answer is Parker Solar Probe. Helios 2 set the old record in 1976, BepiColombo is a Mercury mission, and Solar Orbiter is ESA-led."
    };

    void Start()
    {
        if (names.Length != questions.Length * 4 || cases.Length != questions.Length * 4)
            Debug.LogWarning("Identify bank: names and cases need 4 per question");
    }

    public int GetCount()
    {
        return questions.Length;
    }

    public string GetName(int p, int i)
    {
        int index = p * 4 + i;
        if (index < 0 || index >= names.Length) return "";
        return names[index];
    }

    public string GetCase(int p, int i)
    {
        int index = p * 4 + i;
        if (index < 0 || index >= cases.Length) return "";
        return cases[index];
    }
}
