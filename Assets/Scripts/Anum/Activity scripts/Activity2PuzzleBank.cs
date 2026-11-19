using UdonSharp;
using UnityEngine;

public class Activity2PuzzleBank : UdonSharpBehaviour
{
    public string[] titles = new string[]
    {
        "The Apollo 11 broadcast",
        "The first quasar",
        "From black holes to Wi-Fi",
        "Australian radio telescopes",
        "The birth of radio astronomy"
    };

    [TextArea(2, 4)]
    public string[] questions = new string[]
    {
        "Place the moments from the Apollo 11 moonwalk broadcast in the order they occurred .",
        "Place the steps in the discovery of the first quasar, 3C 273, in the order they occurred.",
        "Place these in order of how a search for exploding black holes end up giving us Wi-Fi?",
        "Put these Australian radio astronomy milestones in the order in which they happened.",
        "Place these moments from the start of radio astronomy in Sydney in the order they happened."
    };

    public string[] sphereLabels = new string[]
    {
        "Fire at Tidbinbilla",
        "Camera on",
        "One small step",
        "Parkes takes over",

        "The Moon passes in front",
        "A spectrum at Palomar",
        "The redshift clicks",
        "Published in Nature",

        "Black holes might explode",
        "The search",
        "The patent",
        "The first chip",

        "Parkes opens",
        "The Compact Array opens",
        "The Murchison observatory opens",
        "SKA-Low construction starts",

        "Noise at sunrise",
        "Sunspots and radio noise",
        "Sunrise on the cliff",
        "A big new idea"
    };

    [TextArea(2, 5)]
    public string[] sphereInfoTexts = new string[]
    {
        "Because of a fire at the Tidbinbilla tracking station there was a forced change of plan to swap with Honeysuckle Creek, which will now follow the lunar module during the moonwalk.",
        "Buzz Aldrin turns on the lunar module’s TV camera while the Parkes radio telescope battles strong winds, outside of its safety limits.",
        "When Neil Armstrong steps onto the Moon. The pictures come through Honeysuckle Creek near Canberra which all of the world see.",
        "About nine minutes (8 minutes 51 seconds) into the broadcast, the main detector at Parkes locks on and the NASA one switches to the Parkes pictures for the rest of the moonwalk.",

        "At Parkes astronomers time the exact moments a radio source called 3C 273 disappears behind the Moon and then reappears, pinning down its exact position.",
        "Maarten Schmidt takes a spectrum of a starlike object near that position at the Palomar 200 inch telescope.",
        "Schmidt realises the strange lines  shifted far to the red are ordinary hydrogen. The object is enormously far away.",
        "Nature publishes the Parkes position and Schmidt's redshift together in the same issue.",

        "Stephen Hawking made a prediction that small black holes could slowly evaporate and give off radio signals as they vanish.",
        "When John O'Sullivan and colleagues search for these faint smeared signals, they could not find the black holes but they got very good at cleaning up messy radio data.",
        "CSIRO files an Australian patent on fast reliable wireless networking based on ideas developed through radio astronomy.",
        "The world's first chip for the 802.11a Wi-Fi standard was demonstrated by Radiata, which is a CSIRO and Macquarie University spin-off .",

        "A 64 metre dish near Parkes is officially opened by the Governor General. which was named Murriyang in 2020 by Wiradjuri elders.",
        "Prime Minister Bob Hawke officially opened six 22 metre dishes at Narrabri.",
        "A remote radio quiet observatory which is home to ASKAP formally opened on Wajarri Yamaji Country in Western Australia.",
        "Construction on SKA-Low begins, which is part of the world's largest radio telescope project, located at Inyarrimanha Ilgari Bundara in Western Australia.",

        "Strange radio noise was noticed by radar operators on Norfolk Island each time the Sun rises.",
        "Air force radar was used at Collaroy, by Joe Pawsey, Ruby Payne Scott and Lindsay McCready to find out that the Sun's radio noise rises and falls with sunspot activity.",
        "In early 1946, Ruby Payne Scott uses an Army radar on the cliffs at Dover Heights to make what were probably the first interferometer observations in radio astronomy.",
        "The idea behind radio arrays like ASKAP was first described as aperture synthesis by Payne Scott and Pawsey."
    };

    [TextArea(2, 4)]
    public string[] hint1 = new string[]
    {
        "One of these happened a few days before the moonwalk.",
        "You need to know where something is before you can point a telescope at it.",
        "The theory usually comes before anyone goes looking for it.",
        "Parkes was used for the Apollo 11 broadcast so it must have been around by 1969.",
        "Someone had to notice something odd before anyone set out to study it."
    };

    [TextArea(2, 4)]
    public string[] hint2 = new string[]
    {
        "Parkes's main detector could only used once the Moon had risen high enough.",
        "A discovery needs to be made before the scientists can publish it.",
        "You can not build a product before the idea has been worked out and is protected.",
        "The Compact Array was opened in 1988 which is Australia's 200th anniversary.",
        "The big idea came after the cliff top experiments showed what was possible."
    };

    [TextArea(3, 6)]
    public string[] correctText = new string[]
    {
        "That is right! The Tidbinbilla fire changed the plan, Aldrin turned on the camera, Honeysuckle Creek carried the first step, and Parkes took over 8 minutes 51 seconds in. The film The Dish shows Parkes broadcasting the first step, but that part is not quite true.",
        "That is Correct! Parkes timed the Moon passing in front of 3C 273 in 1962, Schmidt took the spectrum, worked out the redshift on 5 February 1963, and the papers came out together that March.",
        "Right! Hawking's 1974 idea led O'Sullivan's team to search for exploding black holes. They never found them, but their methods led to CSIRO's 1992 patent application for wireless networking and Radiata's first 802.11a chip.",
        "Correct! Parkes opened in 1961, the Compact Array in 1988, the Murchison observatory in 2012, and SKA-Low construction started in December 2022.",
        "Correct! Radar operators noticed noise at sunrise in early 1945, Pawsey's team linked it to sunspots later that year, Payne-Scott made the cliff top interferometer observations in early 1946, and by mid 1946 she and Pawsey had described aperture synthesis."
    };

    [TextArea(3, 6)]
    public string[] revealText = new string[]
    {
        "The order: the fire at Tidbinbilla, Aldrin switching on the camera, the first step seen through Honeysuckle Creek, then Parkes taking over 8 minutes 51 seconds in.",
        "The order: the Parkes occultation in 1962, Schmidt's spectrum at Palomar, the redshift on 5 February 1963, then the Nature papers in March 1963.",
        "The order: Hawking's prediction in 1974, O'Sullivan's search for exploding black holes, CSIRO's patent application in 1992, then Radiata's first 802.11a chip.",
        "The order: Parkes in 1961, the Compact Array in 1988, the Murchison observatory in 2012, then SKA-Low construction in 2022.",
        "The order: noise at sunrise on Norfolk Island in 1945, the Collaroy sunspot work later in 1945, the Dover Heights observations in early 1946, then aperture synthesis in mid 1946."
    };

    void Start()
    {
        if (sphereLabels.Length != questions.Length * 4 || sphereInfoTexts.Length != questions.Length * 4)
            Debug.LogWarning("Activity2PuzzleBank: each puzzle needs exactly 4 sphere entries");
    }

    public int GetCount()
    {
        return questions.Length;
    }

    public string GetSphereLabel(int puzzle, int slot)
    {
        int index = puzzle * 4 + slot;
        if (index < 0 || index >= sphereLabels.Length) return "";
        return sphereLabels[index];
    }

    public string GetSphereInfo(int puzzle, int slot)
    {
        int index = puzzle * 4 + slot;
        if (index < 0 || index >= sphereInfoTexts.Length) return "";
        return sphereInfoTexts[index];
    }
}