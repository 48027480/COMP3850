using UdonSharp;
using UnityEngine;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class EarthQuizBank : UdonSharpBehaviour
{
    public string[] boothNames = new string[]
    {
        "Oceans",
        "Volcanoes and Rocks",
        "Weather and Climate",
        "Living Earth",
        "Places on Earth"
    };

    [TextArea(2, 4)]
    public string[] questions = new string[]
    {
        "About how much of Earth's surface is covered by ocean?",
        "What is the deepest known point in Earth's oceans?",
        "Which is the largest ocean on Earth?",
        "What is the longest mountain range on Earth?",
        "Where does a large share of the oxygen we breathe come from?",
        "Why is the sea salty?",
        "In April 2024, NOAA confirmed the fourth global coral bleaching event. What mainly causes coral bleaching?",
        "In 2024, the world's largest known coral, longer than a blue whale, was found near which country?",
        "In January 2024, scientists mapped the largest known deep-sea coral reef habitat. Where is it?",
        "Since 2023, iceberg A23a has made the news as one of the world's largest icebergs. Where did it come from?",

        "Since December 2023, a series of eruptions near the town of Grindavik has been in the news. Which country is it in?",
        "The 2022 Hunga Tonga eruption surprised scientists by blasting a huge amount of what into the upper atmosphere?",
        "Where do most of the world's earthquakes happen?",
        "Is Earth's inner core solid or liquid?",
        "Measured from its base on the sea floor, which mountain is the tallest on Earth?",
        "Some of the oldest known pieces of Earth are tiny zircon crystals about 4.4 billion years old. Where were they found?",
        "Uluru in central Australia is made mostly of which kind of rock?",
        "In 2025, Kilauea volcano in Hawaii erupted in dozens of short episodes. What were they known for?",
        "In July 2025, a magnitude 8.8 earthquake set off tsunami alerts across the Pacific. Where did it strike?",
        "A 2024 study suggested Mount Everest is rising a little faster than expected. What helps push it up?",

        "According to Europe's Copernicus climate service, which is the warmest year on record so far?",
        "In 2023, a UN-backed report said the ozone hole over Antarctica should recover to 1980 levels around when?",
        "How hot can a lightning bolt get compared with the surface of the Sun?",
        "The official record for the hottest air temperature, 56.7 C, is held by which place?",
        "The coldest temperature ever officially recorded, minus 89.2 C, was measured where?",
        "What is the difference between a hurricane, a typhoon and a tropical cyclone?",
        "In June 2023, NOAA announced that El Nino had arrived. What is El Nino?",
        "In April 2024, the United Arab Emirates had its heaviest rainfall since records began in about which year?",
        "In 2023, Antarctic sea ice set a record. What kind of record?",
        "How does a rainbow form?",

        "Which is the largest animal alive today?",
        "In 2025, a young member of which animal species was filmed alive in the deep sea for the first time?",
        "In 2023, an egg-laying mammal named after David Attenborough was recorded for the first time in over 60 years. What is it?",
        "Which animal is the longest-living vertebrate known to science?",
        "Which kind of tree is the tallest known living tree?",
        "Which trees include some of the oldest known living things, nearly 5,000 years old?",
        "About what share of the world's leading food crops benefit from animal pollinators like bees?",
        "The platypus is a mammal. What unusual thing does it do?",
        "How many hearts does an octopus have?",
        "Which is the largest tropical rainforest on Earth?",

        "Which is the largest desert on Earth?",
        "In 2023, geologists finished mapping Zealandia. What is Zealandia?",
        "Which is the largest island on Earth that is not a continent?",
        "Which is the largest lake on Earth by surface area?",
        "Where is the lowest point on land?",
        "Which is the driest non-polar desert on Earth?",
        "Which mountain reaches the highest point above sea level?",
        "The World Magnetic Model released in December 2024 shows Earth's magnetic north pole drifting towards where?",
        "Where is most of the world's ice?",
        "The Great Barrier Reef, the largest coral reef system on Earth, lies off which Australian state?"
    };

    public string[] optionA = new string[]
    {
        "About 71 percent",
        "The Java Trench",
        "The Pacific Ocean",
        "The Andes",
        "Tiny ocean plants called phytoplankton",
        "Fish release salt into the water",
        "Water staying too warm for too long",
        "Iceland",
        "Off the south-east coast of the United States",
        "Antarctica",

        "Italy",
        "Water vapour",
        "Around the edges of the Pacific Ocean, the Ring of Fire",
        "Liquid",
        "Mauna Kea, Hawaii",
        "Jack Hills, Western Australia",
        "Granite",
        "Lava fountains hundreds of metres high",
        "Off the Kamchatka Peninsula",
        "A nearby river wearing rock away, so the land springs up",

        "2016",
        "Around 2066",
        "About five times hotter",
        "Alice Springs, Australia",
        "Vostok Station, Antarctica",
        "They are the same kind of storm, named by where it happens",
        "Unusually warm surface water in the tropical Pacific that shifts weather around the world",
        "1949",
        "A record low amount of winter sea ice",
        "Sunlight bends and splits into colours inside raindrops",

        "The blue whale",
        "The colossal squid",
        "A penguin",
        "The Greenland shark",
        "The coast redwood",
        "Great Basin bristlecone pines",
        "About three out of four",
        "It lays eggs",
        "One",
        "The Amazon rainforest",

        "The Sahara",
        "A mostly underwater continent around New Zealand",
        "Greenland",
        "Lake Superior",
        "The shore of the Dead Sea",
        "The Atacama Desert, Chile",
        "K2",
        "Towards Siberia",
        "Antarctica",
        "Queensland"
    };

    public string[] optionB = new string[]
    {
        "About 50 percent",
        "Challenger Deep, in the Mariana Trench",
        "The Atlantic Ocean",
        "The mid-ocean ridge, which is mostly underwater",
        "Desert cactus plants",
        "Rivers carry dissolved minerals from rocks into the sea",
        "Too much rain falling on the reef",
        "Solomon Islands",
        "Under the Antarctic ice",
        "Greenland",

        "Iceland",
        "Sand",
        "In the middle of the continents",
        "Solid",
        "Mount Kilimanjaro",
        "The Sahara Desert",
        "Sandstone",
        "Ash clouds that covered all of Hawaii",
        "In the middle of the Atlantic Ocean",
        "Snow piling up on its summit",

        "2024",
        "It has already fully healed",
        "About half as hot",
        "Death Valley, USA",
        "A village in northern Canada",
        "Typhoons are always much smaller",
        "A cold wind that blows over Spain",
        "1999",
        "A record high amount of winter sea ice",
        "Light bounces off clouds of dust",

        "The African elephant",
        "The giant Pacific octopus",
        "A long-beaked echidna",
        "The Galapagos giant tortoise",
        "The mountain ash",
        "English oak trees",
        "Fewer than one in ten",
        "It breathes through its tail",
        "Three",
        "The Daintree rainforest",

        "Antarctica",
        "A new volcanic island",
        "New Guinea",
        "The Caspian Sea",
        "Death Valley, USA",
        "The Gobi Desert",
        "Mount Everest",
        "Towards southern Australia",
        "Greenland",
        "Tasmania"
    };

    public int[] correct = new int[]
    {
        0, 1, 0, 1, 0, 1, 0, 1, 0, 0,
        1, 0, 0, 1, 0, 0, 1, 0, 0, 0,
        1, 0, 0, 1, 0, 0, 0, 0, 0, 0,
        0, 0, 1, 0, 0, 0, 0, 0, 1, 0,
        1, 0, 0, 1, 0, 0, 1, 0, 0, 0
    };

    [TextArea(2, 4)]
    public string[] facts = new string[]
    {
        "About 71 percent of Earth's surface is covered by water, and almost all of it is ocean.",
        "Challenger Deep is nearly 11 kilometres deep, deeper than Mount Everest is tall.",
        "The Pacific is the biggest ocean, larger than all of Earth's land put together.",
        "The mid-ocean ridge runs about 65,000 kilometres around the globe, almost all of it underwater.",
        "Scientists estimate that roughly half of Earth's oxygen comes from ocean plankton.",
        "Rain wears down rocks, and rivers carry the dissolved minerals to the sea, where the salt builds up.",
        "When water stays too warm, corals push out the tiny algae living inside them and turn white.",
        "The coral was found near the Solomon Islands in 2024 and is around 300 years old.",
        "The reef habitat, nicknamed Million Mounds, stretches off the coast from Florida to South Carolina.",
        "A23a broke off Antarctica's Filchner Ice Shelf in 1986 and stayed stuck on the sea floor for over 30 years.",

        "The eruptions on Iceland's Reykjanes Peninsula forced the town of Grindavik to be evacuated.",
        "The eruption added roughly 10 percent more water vapour to the stratosphere.",
        "About 90 percent of the world's earthquakes happen along the Ring of Fire.",
        "The inner core is solid metal, while the outer core around it is liquid.",
        "Mauna Kea rises about 10,000 metres from the sea floor, though most of it is underwater.",
        "These crystals from the Jack Hills are among the oldest known pieces of Earth's crust.",
        "Uluru is made of a sandstone called arkose, while nearby Kata Tjuta is made of conglomerate.",
        "Some of Kilauea's 2025 lava fountains shot about 300 metres into the air.",
        "The earthquake struck off Russia's Kamchatka Peninsula and was one of the strongest ever recorded.",
        "As the Arun river carves away rock nearby, the land around Everest slowly rebounds upwards.",

        "Copernicus found 2024 was the warmest year on record and the first calendar year above 1.5 C.",
        "The ozone layer is healing because ozone-damaging chemicals were phased out.",
        "Lightning can reach about 28,000 C, roughly five times hotter than the Sun's surface.",
        "The World Meteorological Organization lists 56.7 C, measured in Death Valley in 1913.",
        "Russia's Vostok Station in Antarctica measured minus 89.2 C in July 1983.",
        "They are all the same kind of storm. Only the region decides the name.",
        "El Nino brings warmer water to the tropical Pacific and changes rainfall and temperatures in many countries.",
        "It was the UAE's heaviest rainfall since records began in 1949, flooding parts of Dubai.",
        "In 2023, Antarctic sea ice grew to its smallest winter maximum since satellite records began.",
        "Each raindrop bends and splits sunlight like a tiny prism, spreading it into colours.",

        "Blue whales can grow to around 30 metres long, bigger than any other animal alive.",
        "The young colossal squid was filmed in 2025 near the South Sandwich Islands, about 600 metres deep.",
        "Camera traps in Indonesia's Cyclops Mountains caught the echidna on film for the first time.",
        "Greenland sharks can live for at least 270 years, and possibly much longer.",
        "The tallest known tree is a coast redwood in California, about 116 metres tall.",
        "Bristlecone pines grow in the dry mountains of the western United States.",
        "About three out of four of the world's leading food crops benefit from pollinators.",
        "The platypus and the echidna are monotremes, the only mammals that lay eggs.",
        "An octopus has three hearts. Two pump blood through the gills and one pumps it around the body.",
        "The Amazon is the largest tropical rainforest, spread across nine countries and territories.",

        "A desert is a place with very little rain or snow, so cold, dry Antarctica is the largest.",
        "Zealandia is about 94 percent underwater, and New Zealand is part of the land above the water.",
        "Greenland is the largest island. Australia is counted as a continent instead.",
        "The Caspian Sea is called a sea, but it is really the world's largest lake.",
        "The shore of the Dead Sea lies more than 400 metres below sea level.",
        "Some weather stations in the Atacama have recorded almost no rain for years.",
        "Mount Everest stands about 8,849 metres above sea level.",
        "Earth's magnetic north pole has moved from northern Canada towards Siberia over recent decades.",
        "Antarctica holds about 90 percent of all the ice on Earth.",
        "The Great Barrier Reef stretches more than 2,300 kilometres along the coast of Queensland."
    };

    public AudioClip[] questionClips;

    public int GetCount()
    {
        return questions.Length;
    }
}
