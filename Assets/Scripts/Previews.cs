using System.Collections.Generic;
using UnityEngine;

public class Previews : MonoBehaviour
{
    public MeshPreview Easel;
    public MeshPreview SleevelessShirt;
    public MeshPreview TShirt;
    public MeshPreview LongSleeveShirt;
    public MeshPreview Pullover;
    public MeshPreview Hoodie;
    public MeshPreview Coat;
    public MeshPreview SleevelessDress;
    public MeshPreview ShortSleeveDress;
    public MeshPreview LongSleeveDress;
    public MeshPreview RoundDress;
    public MeshPreview BalloonDress;
    public MeshPreview Robe;
    public MeshPreview BrimmedCap;
    public MeshPreview KnitCap;
    public MeshPreview BrimmedHat;
    public MeshPreview _3DSCapHorn;
    public MeshPreview _3DSCapSphere;
    public MeshPreview _3DSOnePieceNoSleeve;
    public MeshPreview _3DSOnePieceShortSleeve;
    public MeshPreview _3DSOnePieceLongSleeve;
    public MeshPreview _3DSShirtNoSleeve;
    public MeshPreview _3DSShirtShortSleeve;
    public MeshPreview _3DSShirtLongSleeve;
    public MeshPreview _3DSStandee;
    public MeshPreview Standee;
    public MeshPreview Flag;
    public MeshPreview Umbrella;
    public MeshPreview Fan;

    public Texture2D SleevelessShirtLines;
    public Texture2D TShirtLines;
    public Texture2D LongSleeveShirtLines;
    public Texture2D PulloverLines;
    public Texture2D HoodieLines;
    public Texture2D CoatLines;
    public Texture2D SleevelessDressLines;
    public Texture2D ShortSleeveDressLines;
    public Texture2D LongSleeveDressLines;
    public Texture2D RoundDressLines;
    public Texture2D BalloonDressLines;
    public Texture2D RobeLines;
    public Texture2D BrimmedCapLines;
    public Texture2D KnitCapLines;
    public Texture2D BrimmedHatLines;
    public Texture2D _3DSCapHornLines;
    public Texture2D _3DSCapSphereLines;
    public Texture2D _3DSOnePieceNoSleeveLines;
    public Texture2D _3DSOnePieceShortSleeveLines;
    public Texture2D _3DSOnePieceLongSleeveLines;
    public Texture2D _3DSShirtNoSleeveLines;
    public Texture2D _3DSShirtShortSleeveLines;
    public Texture2D _3DSShirtLongSleeveLines;
    public Texture2D _3DSStandeeLines;
    public Texture2D StandeeLines;
    public Texture2D FlagLines;
    public Texture2D UmbrellaLines;
    public Texture2D FanLines;

    public Texture2D SleevelessShirtUVs;
    public Texture2D TShirtUVs;
    public Texture2D LongSleeveShirtUVs;
    public Texture2D PulloverUVs;
    public Texture2D HoodieUVs;
    public Texture2D CoatUVs;
    public Texture2D SleevelessDressUVs;
    public Texture2D ShortSleeveDressUVs;
    public Texture2D LongSleeveDressUVs;
    public Texture2D RoundDressUVs;
    public Texture2D BalloonDressUVs;
    public Texture2D RobeUVs;
    public Texture2D BrimmedCapUVs;
    public Texture2D KnitCapUVs;
    public Texture2D BrimmedHatUVs;
    public Texture2D _3DSCapHornUVs;
    public Texture2D _3DSCapSphereUVs;
    public Texture2D _3DSOnePieceNoSleeveUVs;
    public Texture2D _3DSOnePieceShortSleeveUVs;
    public Texture2D _3DSOnePieceLongSleeveUVs;
    public Texture2D _3DSShirtNoSleeveUVs;
    public Texture2D _3DSShirtShortSleeveUVs;
    public Texture2D _3DSShirtLongSleeveUVs;
    public Texture2D _3DSStandeeUVs;
    public Texture2D StandeeUVs;
    public Texture2D FlagUVs;
    public Texture2D UmbrellaUVs;
    public Texture2D FanUVs;

    public Dictionary<DesignPattern.TypeEnum, MeshPreview> AllPreviews;
    public Dictionary<DesignPattern.TypeEnum, Texture2D> AllUVs;
    public Dictionary<DesignPattern.TypeEnum, Texture2D> AllLines;
    
    // Start is called before the first frame update
    void Start()
    {
        AllPreviews = new Dictionary<DesignPattern.TypeEnum, MeshPreview>()
        {
            { DesignPattern.TypeEnum.SimplePattern, Easel },
            { DesignPattern.TypeEnum.Tanktop, SleevelessShirt },
            { DesignPattern.TypeEnum.TShirt, TShirt },
            { DesignPattern.TypeEnum.LongSleeveShirt, LongSleeveShirt },
            { DesignPattern.TypeEnum.Pullover, Pullover },
            { DesignPattern.TypeEnum.Hoodie, Hoodie },
            { DesignPattern.TypeEnum.Coat, Coat },
            { DesignPattern.TypeEnum.SleevelessDress, SleevelessDress },
            { DesignPattern.TypeEnum.ShortSleeveDress, ShortSleeveDress },
            { DesignPattern.TypeEnum.LongSleeveDress, LongSleeveDress },
            { DesignPattern.TypeEnum.RoundDress, RoundDress },
            { DesignPattern.TypeEnum.BalloonDress, BalloonDress },
            { DesignPattern.TypeEnum.Robe, Robe },
            { DesignPattern.TypeEnum.BrimmedCap, BrimmedCap },
            { DesignPattern.TypeEnum.KnitCap, KnitCap },
            { DesignPattern.TypeEnum.BrimmedHat, BrimmedHat },
            { DesignPattern.TypeEnum.Hat3DS, _3DSCapSphere },
            { DesignPattern.TypeEnum.HornHat3DS, _3DSCapHorn },
            { DesignPattern.TypeEnum.LongSleeveDress3DS, _3DSOnePieceLongSleeve },
            { DesignPattern.TypeEnum.ShortSleeveDress3DS, _3DSOnePieceShortSleeve },
            { DesignPattern.TypeEnum.NoSleeveDress3DS, _3DSOnePieceNoSleeve },
            { DesignPattern.TypeEnum.LongSleeveShirt3DS, _3DSShirtLongSleeve },
            { DesignPattern.TypeEnum.ShortSleeveShirt3DS, _3DSShirtShortSleeve },
            { DesignPattern.TypeEnum.NoSleeveShirt3DS, _3DSShirtNoSleeve },
            { DesignPattern.TypeEnum.Standee3DS, _3DSStandee },
            { DesignPattern.TypeEnum.Standee, Standee },
            { DesignPattern.TypeEnum.Umbrella, Umbrella },
            { DesignPattern.TypeEnum.Flag, Flag },
            { DesignPattern.TypeEnum.Fan, Fan },
        };

        AllLines = new Dictionary<DesignPattern.TypeEnum, Texture2D>()
        {
            { DesignPattern.TypeEnum.Tanktop, SleevelessShirtLines },
            { DesignPattern.TypeEnum.TShirt, TShirtLines },
            { DesignPattern.TypeEnum.LongSleeveShirt, LongSleeveShirtLines },
            { DesignPattern.TypeEnum.Pullover, PulloverLines },
            { DesignPattern.TypeEnum.Hoodie, HoodieLines },
            { DesignPattern.TypeEnum.Coat, CoatLines },
            { DesignPattern.TypeEnum.SleevelessDress, SleevelessDressLines },
            { DesignPattern.TypeEnum.ShortSleeveDress, ShortSleeveDressLines },
            { DesignPattern.TypeEnum.LongSleeveDress, LongSleeveDressLines },
            { DesignPattern.TypeEnum.RoundDress, RoundDressLines },
            { DesignPattern.TypeEnum.BalloonDress, BalloonDressLines },
            { DesignPattern.TypeEnum.Robe, RobeLines },
            { DesignPattern.TypeEnum.BrimmedCap, BrimmedCapLines },
            { DesignPattern.TypeEnum.KnitCap, KnitCapLines },
            { DesignPattern.TypeEnum.BrimmedHat, BrimmedHatLines },
            { DesignPattern.TypeEnum.Hat3DS, _3DSCapSphereLines },
            { DesignPattern.TypeEnum.HornHat3DS, _3DSCapHornLines },
            { DesignPattern.TypeEnum.LongSleeveDress3DS, _3DSOnePieceLongSleeveLines },
            { DesignPattern.TypeEnum.ShortSleeveDress3DS, _3DSOnePieceShortSleeveLines },
            { DesignPattern.TypeEnum.NoSleeveDress3DS, _3DSOnePieceNoSleeveLines },
            { DesignPattern.TypeEnum.LongSleeveShirt3DS, _3DSShirtLongSleeveLines },
            { DesignPattern.TypeEnum.ShortSleeveShirt3DS, _3DSShirtShortSleeveLines },
            { DesignPattern.TypeEnum.NoSleeveShirt3DS, _3DSShirtNoSleeveLines },
            { DesignPattern.TypeEnum.Standee3DS, _3DSStandeeLines },
            { DesignPattern.TypeEnum.Standee, StandeeLines },
            { DesignPattern.TypeEnum.Umbrella, UmbrellaLines },
            { DesignPattern.TypeEnum.Flag, FlagLines },
            { DesignPattern.TypeEnum.Fan, FanLines },
        };

        AllUVs = new Dictionary<DesignPattern.TypeEnum, Texture2D>()
        {
            { DesignPattern.TypeEnum.Tanktop, SleevelessShirtUVs },
            { DesignPattern.TypeEnum.TShirt, TShirtUVs },
            { DesignPattern.TypeEnum.LongSleeveShirt, LongSleeveShirtUVs },
            { DesignPattern.TypeEnum.Pullover, PulloverUVs },
            { DesignPattern.TypeEnum.Hoodie, HoodieUVs },
            { DesignPattern.TypeEnum.Coat, CoatUVs },
            { DesignPattern.TypeEnum.SleevelessDress, SleevelessDressUVs },
            { DesignPattern.TypeEnum.ShortSleeveDress, ShortSleeveDressUVs },
            { DesignPattern.TypeEnum.LongSleeveDress, LongSleeveDressUVs },
            { DesignPattern.TypeEnum.RoundDress, RoundDressUVs },
            { DesignPattern.TypeEnum.BalloonDress, BalloonDressUVs },
            { DesignPattern.TypeEnum.Robe, RobeUVs },
            { DesignPattern.TypeEnum.BrimmedCap, BrimmedCapUVs },
            { DesignPattern.TypeEnum.KnitCap, KnitCapUVs },
            { DesignPattern.TypeEnum.BrimmedHat, BrimmedHatUVs },
            { DesignPattern.TypeEnum.Hat3DS, _3DSCapSphereUVs },
            { DesignPattern.TypeEnum.HornHat3DS, _3DSCapHornUVs },
            { DesignPattern.TypeEnum.LongSleeveDress3DS, _3DSOnePieceLongSleeveUVs },
            { DesignPattern.TypeEnum.ShortSleeveDress3DS, _3DSOnePieceShortSleeveUVs },
            { DesignPattern.TypeEnum.NoSleeveDress3DS, _3DSOnePieceNoSleeveUVs },
            { DesignPattern.TypeEnum.LongSleeveShirt3DS, _3DSShirtLongSleeveUVs },
            { DesignPattern.TypeEnum.ShortSleeveShirt3DS, _3DSShirtShortSleeveUVs },
            { DesignPattern.TypeEnum.NoSleeveShirt3DS, _3DSShirtNoSleeveUVs },
            { DesignPattern.TypeEnum.Standee3DS, _3DSStandeeUVs },
            { DesignPattern.TypeEnum.Standee, StandeeUVs },
            { DesignPattern.TypeEnum.Umbrella, UmbrellaUVs },
            { DesignPattern.TypeEnum.Flag, FlagUVs },
            { DesignPattern.TypeEnum.Fan, FanUVs },
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
