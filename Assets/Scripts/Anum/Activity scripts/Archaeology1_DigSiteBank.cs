using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class Archaeology1_DigSiteBank : UdonSharpBehaviour
{
    public string[] siteNames = new string[]
    {
        "The Rosetta Stone",
        "Pompeii",
        "The Terracotta Army",
        "Otzi the Iceman",
        "The Dead Sea Scrolls"
    };

    public int[] hotspotTile = new int[] { 2, 0, 3, 1, 0 };

    public string[] surveyText = new string[]
    {
        "Weak signal. Loose sand.",
        "No signal.",
        "Strong anomaly! Something big, flat and solid below.",
        "Weak signal. Old wall rubble?",

        "Strong anomaly! Hollow gaps in a hard layer below.",
        "Weak signal. Thick ash, nothing solid.",
        "No signal.",
        "Weak signal. Small stones.",

        "No signal.",
        "Weak signal. Damp clay.",
        "Weak signal. Tree roots.",
        "Strong anomaly! Lots of solid shapes in a row below.",

        "Weak signal. Loose rocks.",
        "Strong anomaly! Something solid frozen in the ice.",
        "No signal.",
        "Weak signal. Melt water.",

        "Strong anomaly! Rounded shapes in a small hollow.",
        "No signal.",
        "Weak signal. Fallen rock.",
        "Weak signal. Dry sand."
    };

    public string[] trowelText = new string[]
    {
        "Too rough. A trowel could chip the carved letters.",
        "Too rough. The hard ash shell could crack and the hollow inside would be lost.",
        "Too rough. The paint on the clay flakes off very easily.",
        "Too rough. A metal blade could cut the preserved skin.",
        "Too rough. The old scroll would tear."
    };

    public string[] stage1Text = new string[]
    {
        "You feel something flat and hard just under the surface.",
        "The soil here is darker and full of tiny broken pieces.",
        "You hit something hard about a foot down, too wide to be a rock.",
        "The ground here is strange, almost icy and very still.",
        "You find the edge of a clay jar, sealed shut."
    };

    public string[] stage2Text = new string[]
    {
        "Carved lines are showing through the dirt, in neat rows.",
        "You can see the shape of a wall and a painted edge.",
        "A face is emerging, carved in grey clay, life sized.",
        "A hand is visible, skin still intact after all this time.",
        "Inside the jar there is a rolled up sheet of old material."
    };

    public string[] foundText = new string[]
    {
        "A large stone slab, carved with the same message written three different ways.",
        "A hollow space in hardened ash, shaped exactly like a person.",
        "A full size clay soldier, one of thousands standing in rows underground.",
        "A naturally preserved body, frozen in ice for thousands of years.",
        "An ancient scroll, written in a language that has not been used for centuries."
    };

    public string[] hint1 = new string[]
    {
        "Not quite. Think about a stone with the same text in three scripts.",
        "Not quite. What could leave hollow shapes in hard volcanic ash?",
        "Not quite. Which find had thousands of clay figures in rows?",
        "Not quite. Where would a body be kept frozen in ice?",
        "Not quite. Which find was kept dry in sealed clay jars?"
    };

    public string[] hint2 = new string[]
    {
        "Still not it. It was found in Egypt and helped people read hieroglyphs.",
        "Still not it. A Roman town buried when Mount Vesuvius erupted.",
        "Still not it. Clay soldiers guarding an emperor's tomb in China.",
        "Still not it. A man found by hikers high in the Alps.",
        "Still not it. Texts found in caves near Qumran."
    };

    public string[] facts = new string[]
    {
        "Found in 1799 by French soldiers rebuilding a fort near Rosetta, Egypt. Its three matching texts later let scholars crack Egyptian hieroglyphs.",
        "The city of Pompeii was buried in 79 CE when Mount Vesuvius erupted. Plaster poured into the ash later revealed the exact shapes of what had been buried.",
        "Found in 1974 by farmers digging a water well near Xi'an, China. Thousands of life size clay soldiers were buried to guard the tomb of China's first emperor.",
        "Found in 1991 by hikers in the Otztal Alps, on the border of Austria and Italy. The body had been preserved in glacier ice for about 5,300 years.",
        "Found in 1947 by a shepherd exploring a cave near Qumran. The dry climate and sealed clay jars preserved the texts for two thousand years."
    };

    public int GetCount()
    {
        return siteNames.Length;
    }
}
