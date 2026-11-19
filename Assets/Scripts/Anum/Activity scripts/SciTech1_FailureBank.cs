using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SciTech1_FailureBank : UdonSharpBehaviour
{
    string[] titles = new string[]
    {
        "Mars Climate Orbiter, 1999",
        "Ariane 5 Flight 501, 1996",
        "Tacoma Narrows Bridge, 1940",
        "London Millennium Bridge, 2000",
        "de Havilland Comet, 1954"
    };

    string[] summaries = new string[]
    {
        "September 1999. NASA's Mars Climate Orbiter fires its engine to go into orbit around Mars, passes behind the planet and is never heard from again. What went wrong?",
        "4 June 1996. The first Ariane 5 rocket lifts off. About 40 seconds later, at around 3,700 m, it veers off its flight path, breaks up and explodes. Why?",
        "7 November 1940. Four months after opening, the Tacoma Narrows suspension bridge in Washington State twists violently and its main span falls into Puget Sound. What brought it down?",
        "June 2000. London's new Millennium footbridge opens across the Thames and sways sideways under the crowds. It is closed and does not reopen until 2002. What caused the wobble?",
        "1954. Two Comets, the world's first jet airliners, break up in flight over the Mediterranean within three months: one near Elba in January, one near Naples in April. Why?"
    };

    string[] evidence = new string[]
    {
        "The spacecraft's own flight software was written to take thruster data in newton-seconds, exactly as the interface specification said. It did what it was told.",
        "The planned arrival path kept the orbiter well above 80 km, the lowest it was thought able to survive. The path it really flew took it to about 57 km.",
        "The problem did not start on arrival day. The orbiter's path had been drifting across months of small thruster firings on the way to Mars.",

        "For safety the rocket carried two identical inertial reference units. Both shut down in the same way, within moments of each other.",
        "The rocket did not blow up first. It swung off its flight path, and the air forces on it then broke it apart.",
        "The inquiry board found the weather at Kourou that morning was acceptable for launch and played no part in the failure.",

        "The bridge had been closed to traffic about an hour before the main span fell.",
        "Engineers measured the wind at 38 mph at 7:30 a.m. and 42 mph two hours later (about 61 to 68 km/h). Strong, but no storm of the century.",
        "For months, engineers had been recording how the deck moved in the wind, noting the wind speed and the shape of each motion.",

        "Engineers recreated the sway with crowds walking on one span, and later checked the fix by sending about 700 staff across together.",
        "It was fixed by fitting dampers to the structure, 37 viscous dampers among them, without rebuilding the foundations.",
        "The motion that was strongly excited was sideways. Vertical bouncing from footsteps was found not to be a significant factor.",

        "Two different aircraft, on two different flights months apart, broke up in the same way.",
        "Investigators sealed a whole Comet fuselage in a water tank and pressurised and depressurised it again and again. No engines ran and nothing flew, yet the cabin eventually split open.",
        "The test cabin did not burst the first time it was pressurised. It had flown about 1,200 real flights and then survived about 1,800 more cycles in the tank before it failed."
    };

    string[] options = new string[]
    {
        "The spacecraft's own computer or thrusters failed",
        "Mission planners chose an arrival path that was too low",
        "Mars's atmosphere was thicker than expected that day",
        "Ground software gave thruster data in imperial units, not metric",

        "Reused guidance software overflowed on Ariane 5's faster flight path",
        "A random hardware fault in one guidance unit",
        "An engine or booster exploded first",
        "Strong winds pushed it off course",

        "Too much traffic for the deck to carry",
        "An earthquake shook the towers",
        "A violent, once-in-a-century storm",
        "Steady wind set off a self-feeding twisting motion (flutter)",

        "Gusty wind along the river",
        "Walkers unconsciously falling into step with a small sideways sway",
        "A construction defect in the foundations",
        "Vertical bouncing from footsteps",

        "A bomb planted on board",
        "The engines failed",
        "Metal fatigue from the cabin being pressurised again and again",
        "The cabin was simply too weak for its flying pressure"
    };

    int[] correct = new int[] { 3, 0, 3, 1, 2 };

    string[] hint1 = new string[]
    {
        "Each piece of evidence clears one suspect. Which one is left standing?",
        "Each piece of evidence clears one suspect. Think about what two identical units have in common.",
        "Each piece of evidence clears one suspect. What was the bridge doing in the wind for months?",
        "Each piece of evidence clears one suspect. What changed when crowds were on it?",
        "Each piece of evidence clears one suspect. Strong once is not the same as strong thousands of times."
    };

    string[] hint2 = new string[]
    {
        "Look at where the numbers came from before they reached the spacecraft. Two teams, two systems of measurement.",
        "Identical units running identical software will fail the same way. Ariane 5 flew differently from Ariane 4.",
        "The wind was not extreme. The motion fed on itself.",
        "The bridge only moved sideways, and only with people walking on it.",
        "Every flight stretched the cabin a little. What does that do to metal over time?"
    };

    string[] correctTexts = new string[]
    {
        "Right. A ground software file called \"Small Forces\" gave impulse in pound-force seconds instead of newton-seconds, so every small firing was underestimated. NASA's board named this the root cause.",
        "Right. The guidance software came from Ariane 4. Ariane 5's early flight gives much higher horizontal velocity, so a 64-bit value overflowed a 16-bit integer and both units stopped. The board reproduced this in simulation.",
        "Right. The deck's twisting drew energy from the wind itself, which is called aeroelastic flutter. The collapse changed how suspension bridges are designed for wind.",
        "Right. Arup called it synchronous lateral excitation: the deck sways a little, people adjust their steps to it, and that pushes it further.",
        "Right. Each pressurisation cycle grew tiny cracks at the corners of openings in the cabin until it tore open. The tank cabin failed at the corner of a window. The inquiry put the failures down to metal fatigue."
    };

    string[] revealTexts = new string[]
    {
        "The answer on record: ground software gave thruster impulse in pound-force seconds instead of newton-seconds (Mishap Investigation Board, 1999).",
        "The answer on record: reused Ariane 4 software overflowed when converting the horizontal bias value, stopping both guidance units (Inquiry Board, 1996).",
        "The answer on record: aeroelastic flutter in a wind of about 40 to 42 mph (WSDOT).",
        "The answer on record: synchronous lateral excitation, people falling into step with a sideways sway (Arup).",
        "The answer on record: metal fatigue from repeated cabin pressurisation (Court of Inquiry, C.A.P. 127)."
    };

    public int Count() { return titles.Length; }
    public string GetTitle(int c) { return titles[c]; }
    public string GetSummary(int c) { return summaries[c]; }
    public string GetEvidence(int c, int i) { return evidence[c * 3 + i]; }
    public string GetOption(int c, int i) { return options[c * 4 + i]; }
    public int GetCorrect(int c) { return correct[c]; }
    public string GetHint1(int c) { return hint1[c]; }
    public string GetHint2(int c) { return hint2[c]; }
    public string GetCorrectText(int c) { return correctTexts[c]; }
    public string GetRevealText(int c) { return revealTexts[c]; }
}
