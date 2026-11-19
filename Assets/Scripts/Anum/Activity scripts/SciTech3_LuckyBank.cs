using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech3_LuckyBank : UdonSharpBehaviour
{
    string[] titles = new string[]
    {
        "Lucky Breaks: Set 1",
        "Lucky Breaks: Set 2",
        "Lucky Breaks: Set 3",
        "Lucky Breaks: Set 4",
        "Lucky Breaks: Set 5"
    };

    string[] accidents = new string[]
    {
        "A plate of bacteria was left out over a holiday. A stray mould grew on it, and the bacteria around the mould were gone.",
        "A tube was wrapped in black card in a darkened room, yet a coated screen some distance away began to glow.",
        "A student trying to make an anti-malaria drug from coal tar ended up with black sludge that left a strong purple stain.",

        "Crystals put away in a drawer during cloudy weather, with no sunlight at all, still left clear images on wrapped photographic plates.",
        "A gas cylinder still weighed as if it were full, but nothing came out. Cut open, it held a slippery white powder.",
        "A clear plastic tested for gun sights was turned down because it stuck to everything it touched.",

        "A wrong resistor turned a circuit meant to record heart sounds into one that pulsed in a steady rhythm.",
        "An engineer working near a powerful radar tube noticed the chocolate bar in his pocket had melted.",
        "After a day in the lab, a chemist found that the bread roll at his dinner tasted strangely sweet.",

        "A glass flask fell and cracked all over, but the pieces held together in the flask's shape.",
        "A high-pressure reaction meant to make something else left a white, waxy solid inside the steel vessel. A trace of oxygen had got in.",
        "A chemist working on adhesives made a weak one: it held paper, peeled off cleanly and could be stuck down again.",

        "While setting up a lecture demonstration, a professor saw a compass needle swing when he connected a wire to a battery.",
        "Frustrated by slow tests, a researcher gave a heated plastic rod one sudden yank. It stretched about 800% into a material that was mostly air.",
        "A hot soldering iron touched a syringe needle full of ink, and droplets spurted out of the tip."
    };

    string[] options = new string[]
    {
        "X-rays", "Insulin", "Penicillin", "Mauve dye",
        "Super glue", "Radioactivity", "Nylon", "Non-stick PTFE",
        "Microwave oven", "Velcro", "Artificial sweetener", "Pacemaker",
        "Sticky notes", "Polyethylene", "Safety glass", "Kevlar",
        "Inkjet printing", "Lithium-ion battery", "Gore-Tex fabric", "Electromagnetism"
    };

    int[] answers = new int[]
    {
        2, 0, 3,
        1, 3, 0,
        3, 0, 2,
        2, 1, 0,
        3, 2, 0
    };

    string[] facts = new string[]
    {
        "Penicillin: Alexander Fleming, St Mary's Hospital, London, 1928.",
        "X-rays: Wilhelm Rontgen, Wurzburg, 8 November 1895. First Nobel Prize in Physics, 1901.",
        "Mauve: William Henry Perkin, aged 18, 1856. The first commercial synthetic dye.",

        "Radioactivity: Henri Becquerel, Paris, 1896. He had expected sunlight to be needed.",
        "PTFE (Teflon): Roy Plunkett, DuPont, 6 April 1938, while working on refrigerant gases.",
        "Super glue: Harry Coover's team, 1942. He saw its use as an adhesive in 1951, and it went on sale in 1958.",

        "Implantable pacemaker: Wilson Greatbatch, 1956. A 1 megohm resistor instead of a 10 kilohm one.",
        "Microwave oven: Percy Spencer, Raytheon. The patent was filed on 8 October 1945.",
        "Saccharin: Constantin Fahlberg, in Ira Remsen's lab at Johns Hopkins, 1878. Published in 1879.",

        "Laminated safety glass: Edouard Benedictus, 1903. A dried film of cellulose nitrate was inside the flask.",
        "Polyethylene: Eric Fawcett and Reginald Gibson, ICI, March 1933.",
        "Post-it adhesive: Spencer Silver, 3M, 1968. Art Fry found the use in 1974, as a bookmark for his hymn book.",

        "Electromagnetism: Hans Christian Orsted, Copenhagen, April 1820. He published that July.",
        "Expanded PTFE (Gore-Tex): Bob Gore, 1969. The new material was about 70% air.",
        "Thermal inkjet: Canon, 1970s. Canon's first Bubble Jet printer went on sale in 1985."
    };

    string[] hint1 = new string[]
    {
        "Not every name fits. One of the four is a decoy.",
        "Not every name fits. Think about which accident needed no light at all.",
        "Not every name fits. Which accident sounds like it involves electronics?",
        "Not every name fits. Which accident is about something holding together?",
        "Not every name fits. Which one happened in front of an audience?"
    };

    string[] hint2 = new string[]
    {
        "The decoy is Insulin.",
        "The decoy is Nylon.",
        "The decoy is Velcro.",
        "The decoy is Kevlar.",
        "The decoy is the Lithium-ion battery."
    };

    string[] correctTexts = new string[]
    {
        "All three matched. Each card now shows who made the discovery and when.",
        "All three matched. Each card now shows who made the discovery and when.",
        "All three matched. Each card now shows who made the discovery and when.",
        "All three matched. Each card now shows who made the discovery and when.",
        "All three matched. Each card now shows who made the discovery and when."
    };

    string[] revealTexts = new string[]
    {
        "Here are the matches on record. Each card shows who made the discovery and when.",
        "Here are the matches on record. Each card shows who made the discovery and when.",
        "Here are the matches on record. Each card shows who made the discovery and when.",
        "Here are the matches on record. Each card shows who made the discovery and when.",
        "Here are the matches on record. Each card shows who made the discovery and when."
    };

    public int Count() { return titles.Length; }
    public string GetTitle(int r) { return titles[r]; }
    public string GetAccident(int r, int i) { return accidents[r * 3 + i]; }
    public string GetOption(int r, int o) { return options[r * 4 + o]; }
    public int GetAnswer(int r, int i) { return answers[r * 3 + i]; }
    public string GetFact(int r, int i) { return facts[r * 3 + i]; }
    public string GetHint1(int r) { return hint1[r]; }
    public string GetHint2(int r) { return hint2[r]; }
    public string GetCorrectText(int r) { return correctTexts[r]; }
    public string GetRevealText(int r) { return revealTexts[r]; }
}
