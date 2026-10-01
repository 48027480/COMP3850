using UdonSharp;
using UnityEngine;

public class JigsawPuzzleBank : UdonSharpBehaviour
{
    public string[] titles = new string[]
    {
        "Olbers' paradox",
        "The missing matter",
        "The Bullet Cluster",
        "A burst from our own galaxy",
        "Forging heavy elements"
    };

    [TextArea(2, 4)]
    public string[] questions = new string[]
    {
        "If space is filled with stars in every direction, why is the night sky dark?",
        "Astronomers know how much normal matter the universe should contain, but when they counted what they could see, they found less than half of it. Where is the rest?",
        "Two galaxy clusters smashed through each other. Where is most of their mass now, and what is it?",
        "On 28 April 2020, radio telescopes caught a bright, millisecond-long radio burst from inside our own galaxy. What caused it?",
        "In August 2017, there was a glow from a cosmic collision observed by the astronomers, and later they identified freshly made strontium in its light. What collided to produce this?"
    };

    [TextArea(3, 6)]
    public string[] pieceA = new string[]
    {
        "A star looks fainter the farther away it is, but the farther out you look, the more stars there are. The two effects cancel out.",
        "Fast radio bursts are millisecond flashes of radio waves from distant galaxies. And when they pass through ionised gas, their lower frequencies arrive slightly later than their higher ones.",
        "Most of the normal matter in a galaxy cluster is hot gas, around ten times more than the mass in its stars.",
        "Two separate radio telescopes, CHIME in Canada and STARE2 in the USA, have independently detected the burst coming from the same position in the Milky Way.",
        "The LIGO Scientific Collaboration and Virgo Collaboration detectors both picked up gravitational waves from two objects spiralling into each other. And 1.7 seconds later, a satellite saw a gamma-ray burst from the same area."
    };

    [TextArea(3, 6)]
    public string[] pieceB = new string[]
    {
        "The light we see from the most distant galaxies left them over 13 billion years ago, close to when the universe began.",
        "CSIRO's ASKAP radio telescope in Western Australia has pinpointed which galaxies several of these bursts came from, so astronomers knew how far each one had travelled.",
        "The X-ray images show hot gas was slowed down and left behind in the collision, while the galaxies carried on through.",
        "Satellites recorded an X-ray burst from that position at the same moment. The object there had already produced X-ray outbursts in 2014, 2015 and 2016.",
        "The glow faded in the way expected from newly made radioactive heavy elements decaying. In 2019, strontium was identified in its light."
    };

    [TextArea(3, 6)]
    public string[] pieceC = new string[]
    {
        "Dust clouds absorb some of the starlight, but anything that keeps absorbing light heats up and eventually radiates its own glow. And even a limited number of stars would still be enough to light the sky.",
        "The farther a burst had travelled, the bigger its delay. Together, the delays matched the full amount of normal matter the theory predicts.",
        "Gravitational lensing maps where mass is. It shows most of the mass is sitting with the galaxies instead of the gas, far more than the stars could account for.",
        "The object at that position is a neutron star with a surface magnetic field above 10^14 gauss, which is more than a hundred trillion times stronger than Earth's.",
        "The two objects each had roughly 1.2 to 1.6 times the mass of the Sun, which is in line with the masses of known neutron stars."
    };

    public string[] options = new string[]
    {
        "Dust clouds block the light from stars that are far away.",
        "The stars that are far away are too faint to add up to much.",
        "The universe hasn't existed for long enough for light from most stars to reach us.",
        "There aren't enough stars to fill the whole sky.",

        "Hidden inside black holes.",
        "Spread out as thin, ionised gas between galaxies.",
        "It never existed and the prediction was wrong.",
        "It is actually dark matter.",

        "In the hot gas left behind in the collision.",
        "In the stars of the galaxies.",
        "In unseen matter that travelled through with the galaxies.",
        "Spread evenly across the whole region.",

        "Two neutron stars merging.",
        "A magnetar, which is a neutron star with an extremely strong magnetic field.",
        "A distant galaxy far behind the Milky Way.",
        "Radio interference from a satellite or phone.",

        "An ordinary supernova of one massive star.",
        "Two black holes merging.",
        "Two neutron stars merging.",
        "A collapsar, which is a rotating massive star collapsing."
    };

    public int[] correctOptionIndex = new int[] { 2, 1, 2, 1, 2 };

    [TextArea(2, 4)]
    public string[] hint1 = new string[]
    {
        "Try using your clues to rule options out, rather than to prove one right.",
        "Something slowed the radio waves on their journey. Where along the path must it be?",
        "Clue C says where the mass is. Clue A says what the normal matter is. Do they point to the same place?",
        "Clue A tells you where the burst came from, and that it was real. Which options does that rule out?",
        "Clue A says how many objects there were. Which options does that remove?"
    };

    [TextArea(2, 4)]
    public string[] hint2 = new string[]
    {
        "Light takes time to travel. What changes if the universe had a beginning?",
        "Clue C says the matter really is there. Which option fits matter spread thinly along the whole path?",
        "If the mass isn't the gas and isn't the stars, what could pass through a collision without being slowed?",
        "A merger only happens once. What does clue B say about this object's history?",
        "Compare the masses in clue C with what you know about black holes and neutron stars."
    };

    [TextArea(3, 6)]
    public string[] correctText = new string[]
    {
        "Correct! The universe has a definite starting point, so light from most of it hasn't had the time to reach us yet. Its expansion also stretches distant light to longer, dimmer wavelengths, but the finite age is generally seen as the bigger effect.",
        "That's right! The missing matter is extremely thin, ionised gas between galaxies. In 2020, Jean-Pierre Macquart and colleagues measured it using bursts located by CSIRO's ASKAP. The link between distance and delay is now called the Macquart relation.",
        "That is correct. The mass travelled with something unseen that passed through the collision without being slowed, called dark matter. Clowe and colleagues (2006) presented this as direct evidence for dark matter, though some modified-gravity researchers still dispute that reading.",
        "Correct! It was the magnetar SGR 1935+2154. The burst was only about 40 times less energetic than the weakest fast radio burst seen from other galaxies, strong evidence that at least some fast radio bursts come from magnetars. Whether all of them do is still open.",
        "That's right! It was GW170817, two neutron stars merging. It showed that neutron star mergers make heavy elements. Gold and platinum are expected from the same process but weren't directly detected, and some astronomers argue collapsars make most of our galaxy's heavy elements."
    };

    [TextArea(3, 6)]
    public string[] revealText = new string[]
    {
        "The answer: the universe hasn't existed long enough for light from most stars to reach us. Expansion also dims distant light. Dust can't explain it because it would heat up and glow, and even a limited number of stars would still be enough to fill the sky.",
        "The answer: thin, ionised gas spread between galaxies. Fast radio bursts get delayed by this gas as they pass through. Scientists used the bursts which were located by CSIRO's ASKAP to show that the delays add up to the full amount of normal matter predicted by the theory (Macquart et al., 2020).",
        "The answer: unseen matter that travelled with the galaxies, called dark matter. The hot gas holds most of the normal matter but was left behind, while lensing shows the mass stayed with the galaxies (Clowe et al., 2006). Some researchers argue for modified gravity instead.",
        "The answer: a magnetar, SGR 1935+2154. Radio and X-ray bursts came from the same object, which had flared before. It suggests at least some fast radio bursts come from magnetars, though not necessarily all.",
        "The answer: two neutron stars merging, known as GW170817. Gravitational waves showed two objects with neutron-star masses, and strontium was found in the glow. Gold wasn't directly detected, so scientists are still debating how much heavy material comes from these mergers compared with collapsars."
    };

    void Start()
    {
        if (options.Length != questions.Length * 4)
            Debug.LogWarning("Puzzle bank: options needs 4 answers per question");
    }

    public int GetCount()
    {
        return questions.Length;
    }

    public string GetOption(int p, int i)
    {
        int index = p * 4 + i;
        if (index < 0 || index >= options.Length) return "";
        return options[index];
    }
}
