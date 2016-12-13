#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
using Newtonsoft.Json.Linq;


#endregion

using NUnit.Framework;

public class WorldTest  
{
    private World world;

    [Test]
    public void TestWorldLoading()
    {
        ReadWorld();

        Assert.IsNotNull(world);

        world = null;
    }

    [Test]
    public void TestWorldSaving()
    {
        ReadWorld();

        JObject worldJObject = world.ToJson();
        Assert.AreEqual("10", worldJObject["Width"]);
        Assert.AreEqual("10", worldJObject["Height"]);
        Assert.AreEqual("1", worldJObject["Depth"]);

        JArray charactersJArray = (JArray)worldJObject["Characters"];
        Assert.AreEqual(1, charactersJArray.Count);


    }


    private void ReadWorld()
    {
        world = new World();
//        world.ReadJson((JObject)JToken.Parse(worldJsonString));
    }


    private string worldJsonString = @"{
  ""Width"": ""10"",
  ""Height"": ""10"",
  ""Depth"": ""1"",
  ""Rooms"": [
    {},
    {},
    {},
    {},
    {},
    {},
    {}
  ],
  ""Tiles"": [
    {
      ""X"": 1,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 2,
      ""Y"": 4,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 2,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 4,
      ""Type"": ""Floor""
    },
    {
      ""X"": 2,
      ""Y"": 6,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 3,
      ""Y"": 3,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 3,
      ""Y"": 4,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 3,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 3,
      ""Y"": 6,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 3,
      ""Y"": 7,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 2,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 3,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 3,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 4,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 6,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 7,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 2,
      ""Type"": ""Floor""
    },
    {
      ""X"": 4,
      ""Y"": 8,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 5,
      ""Y"": 3,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 5,
      ""Y"": 4,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 5,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 5,
      ""Y"": 6,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 5,
      ""Y"": 7,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 2,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 3,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 7,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 4,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 5,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 6,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 7,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": 6,
      ""Type"": ""Floor""
    },
    {
      ""X"": 6,
      ""Y"": 8,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 7,
      ""Y"": 3,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 7,
      ""Y"": 5,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    },
    {
      ""X"": 7,
      ""Y"": 7,
      ""Z"": 0,
      ""TimesWalked"": 0,
      ""RoomID"": -1,
      ""Type"": ""Floor""
    }
  ],
  ""Inventories"": [],
  ""Furnitures"": [
    {
      ""X"": 3,
      ""Y"": 6,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 3,
      ""Y"": 4,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 5,
      ""Y"": 4,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 5,
      ""Y"": 6,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 3,
      ""Y"": 5,
      ""Z"": 0,
      ""Type"": ""door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 5,
      ""Y"": 5,
      ""Z"": 0,
      ""Type"": ""door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 4,
      ""Y"": 6,
      ""Z"": 0,
      ""Type"": ""airlock_door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 4,
      ""Y"": 4,
      ""Z"": 0,
      ""Type"": ""airlock_door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 3,
      ""Y"": 7,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 5,
      ""Y"": 7,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 3,
      ""Y"": 3,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 5,
      ""Y"": 3,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 4,
      ""Y"": 8,
      ""Z"": 0,
      ""Type"": ""airlock_door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 4,
      ""Y"": 2,
      ""Z"": 0,
      ""Type"": ""airlock_door"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""openness"": ""0""
      }
    },
    {
      ""X"": 2,
      ""Y"": 6,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 2,
      ""Y"": 4,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 1,
      ""Y"": 5,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 6,
      ""Y"": 6,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 7,
      ""Y"": 5,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 6,
      ""Y"": 4,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 7,
      ""Y"": 7,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 6,
      ""Y"": 8,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 6,
      ""Y"": 2,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    },
    {
      ""X"": 7,
      ""Y"": 3,
      ""Z"": 0,
      ""Type"": ""steel_wall"",
      ""Rotation"": 0.0,
      ""Parameters"": {
        ""thermal_diffusivity"": ""0.2""
      }
    }
  ],
  ""Utilities"": [],
  ""RoomBehaviors"": [],
  ""Characters"": [
    {
      ""Name"": ""Koosemose"",
      ""X"": 4,
      ""Y"": 5,
      ""Z"": 0,
      ""Needs"": {
        ""Oxygen"": 0.377170116,
        ""Sleep"": 0.188585058,
        ""Strength"": 3,
        ""Dexterity"": 8,
        ""Constitution"": 16,
        ""Intelligence"": 7,
        ""Wisdom"": 19,
        ""Charisma"": 3
      },
      ""Colors"": {
        ""CharacterColor"": [
          0.32635802,
          0.215369582,
          0.108731747
        ],
        ""UniformColor"": [
          0.1870054,
          0.1870054,
          0.1870054
        ],
        ""SkinColor"": [
          0.929411769,
          0.7490196,
          0.654902
        ]
      },
      ""Stats"": {}
    }
  ],
  ""CameraData"": {
    ""X"": 5.0,
    ""Y"": 5.0,
    ""Z"": -10.0,
    ""ZoomLevel"": 11.0,
    ""ZLevel"": 0,
    ""Presets"": [
      {
        ""X"": 5.0,
        ""Y"": 5.0,
        ""Z"": -10.0,
        ""ZoomLevel"": 11.0
      },
      {
        ""X"": 5.0,
        ""Y"": 5.0,
        ""Z"": -10.0,
        ""ZoomLevel"": 11.0
      },
      {
        ""X"": 5.0,
        ""Y"": 5.0,
        ""Z"": -10.0,
        ""ZoomLevel"": 11.0
      },
      {
        ""X"": 5.0,
        ""Y"": 5.0,
        ""Z"": -10.0,
        ""ZoomLevel"": 11.0
      },
      {
        ""X"": 5.0,
        ""Y"": 5.0,
        ""Z"": -10.0,
        ""ZoomLevel"": 11.0
      }
    ]
  },
  ""Skybox"": ""DSB"",
  ""Wallet"": {},
  ""Scheduler"": []
}";
}
