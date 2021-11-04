using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SavegameInfo
{
    private static List<Header> Headers = new List<Header>()
    {
        new Header(0x67,    0x6F,    2, 0, 2, 0, "1.0.0", 0),
        new Header(0x6D,    0x78,    2, 0, 2, 1, "1.1.0", 1),
        new Header(0x6D,    0x78,    2, 0, 2, 2, "1.1.1", 1),
        new Header(0x6D,    0x78,    2, 0, 2, 3, "1.1.2", 1),
        new Header(0x6D,    0x78,    2, 0, 2, 4, "1.1.3", 1),
        new Header(0x6D,    0x78,    2, 0, 2, 5, "1.1.4", 1),
        new Header(0x20006, 0x20008, 2, 0, 2, 6, "1.2.0", 2),
        new Header(0x20006, 0x20008, 2, 0, 2, 7, "1.2.1", 2),
        new Header(0x40002, 0x40008, 2, 0, 2, 8, "1.3.0", 3),
        new Header(0x40002, 0x40008, 2, 0, 2, 9, "1.3.1", 3),
        new Header(0x50001, 0x5000B, 2, 0, 2, 10, "1.4.0", 4),
        new Header(0x50001, 0x5000B, 2, 0, 2, 11, "1.4.1", 4),
        new Header(0x50001, 0x5000B, 2, 0, 2, 12, "1.4.2", 4),
        new Header(0x60001, 0x6000C, 2, 0, 2, 13, "1.5.0", 5),
        new Header(0x60001, 0x6000C, 2, 0, 2, 14, "1.5.1", 5),
        new Header(0x70001, 0x70006, 2, 0, 2, 15, "1.6.0", 6),
		new Header(0x74001, 0x74005, 2, 0, 2, 16, "1.7.0", 7),
        new Header(0x78001, 0x78001, 2, 0, 2, 17, "1.8.0", 8),
        new Header(0x7C001, 0x7C006, 2, 0, 2, 18, "1.9.0", 9),
        new Header(0x7D001, 0x7D004, 2, 0, 2, 19, "1.10.0", 10),
        new Header(0x7E001, 0x7E001, 2, 0, 2, 20, "1.11.0", 10),
        new Header(0x7E001, 0x7E001, 2, 0, 2, 21, "1.11.1", 10),
        new Header(0x80009, 0x80085, 2, 0, 2, 22, "2.0.0", 11)
    };

    private static List<Info> Infos = new List<Info>()
    {
        new Info(0xAC0938, 0x1D72F0, 0x1DF7C0, 50, 50, new HashRegion[] {
            new HashRegion(0x000108, 0x00010C, 0x1D6D4C),
            new HashRegion(0x1D6E58, 0x1D6E5C, 0x323384),
            new HashRegion(0x4FA2E8, 0x4FA2EC, 0x035AC4),
            new HashRegion(0x52FDB0, 0x52FDB4, 0x03607C),
            new HashRegion(0x565F38, 0x565F3C, 0x035AC4),
            new HashRegion(0x59BA00, 0x59BA04, 0x03607C),
            new HashRegion(0x5D1B88, 0x5D1B8C, 0x035AC4),
            new HashRegion(0x607650, 0x607654, 0x03607C),
            new HashRegion(0x63D7D8, 0x63D7DC, 0x035AC4),
            new HashRegion(0x6732A0, 0x6732A4, 0x03607C),
            new HashRegion(0x6A9428, 0x6A942C, 0x035AC4),
            new HashRegion(0x6DEEF0, 0x6DEEF4, 0x03607C),
            new HashRegion(0x715078, 0x71507C, 0x035AC4),
            new HashRegion(0x74AB40, 0x74AB44, 0x03607C),
            new HashRegion(0x780CC8, 0x780CCC, 0x035AC4),
            new HashRegion(0x7B6790, 0x7B6794, 0x03607C),
            new HashRegion(0x7EC918, 0x7EC91C, 0x035AC4),
            new HashRegion(0x8223E0, 0x8223E4, 0x03607C),
            new HashRegion(0x858460, 0x858464, 0x2684D4)
        }),
        new Info(0xAC2AA0, 0x1D7310, 0x1DF7E0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x000114, 0x1D6D5C),
            new HashRegion(0x1D6E70, 0x1D6E74, 0x323C0C),
            new HashRegion(0x4FAB90, 0x4FAB94, 0x035AFC),
            new HashRegion(0x530690, 0x530694, 0x0362BC),
            new HashRegion(0x566A60, 0x566A64, 0x035AFC),
            new HashRegion(0x59C560, 0x59C564, 0x0362BC),
            new HashRegion(0x5D2930, 0x5D2934, 0x035AFC),
            new HashRegion(0x608430, 0x608434, 0x0362BC),
            new HashRegion(0x63E800, 0x63E804, 0x035AFC),
            new HashRegion(0x674300, 0x674304, 0x0362BC),
            new HashRegion(0x6AA6D0, 0x6AA6D4, 0x035AFC),
            new HashRegion(0x6E01D0, 0x6E01D4, 0x0362BC),
            new HashRegion(0x7165A0, 0x7165A4, 0x035AFC),
            new HashRegion(0x74C0A0, 0x74C0A4, 0x0362BC),
            new HashRegion(0x782470, 0x782474, 0x035AFC),
            new HashRegion(0x7B7F70, 0x7B7F74, 0x0362BC),
            new HashRegion(0x7EE340, 0x7EE344, 0x035AFC),
            new HashRegion(0x823E40, 0x823E44, 0x0362BC),
            new HashRegion(0x85A100, 0x85A104, 0x26899C)
        }),
        new Info(0xACECD0, 0x1D7310, 0x1DF7E0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x000114, 0x1D6D5C),
            new HashRegion(0x1D6E70, 0x1D6E74, 0x323EBC),
            new HashRegion(0x4FAE40, 0x4FAE44, 0x035D2C),
            new HashRegion(0x530B70, 0x530B74, 0x03787C),
            new HashRegion(0x568500, 0x568504, 0x035D2C),
            new HashRegion(0x59E230, 0x59E234, 0x03787C),
            new HashRegion(0x5D5BC0, 0x5D5BC4, 0x035D2C),
            new HashRegion(0x60B8F0, 0x60B8F4, 0x03787C),
            new HashRegion(0x643280, 0x643284, 0x035D2C),
            new HashRegion(0x678FB0, 0x678FB4, 0x03787C),
            new HashRegion(0x6B0940, 0x6B0944, 0x035D2C),
            new HashRegion(0x6E6670, 0x6E6674, 0x03787C),
            new HashRegion(0x71E000, 0x71E004, 0x035D2C),
            new HashRegion(0x753D30, 0x753D34, 0x03787C),
            new HashRegion(0x78B6C0, 0x78B6C4, 0x035D2C),
            new HashRegion(0x7C13F0, 0x7C13F4, 0x03787C),
            new HashRegion(0x7F8D80, 0x7F8D84, 0x035D2C),
            new HashRegion(0x82EAB0, 0x82EAB4, 0x03787C),
            new HashRegion(0x866330, 0x866334, 0x26899C)
        }),
        new Info(0xACED80, 0x1D7310, 0x1DF7E0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1D6D5C),
            new HashRegion(0x1D6E70, 0x323EEC),
            new HashRegion(0x4FAE70, 0x035D2C),
            new HashRegion(0x530BA0, 0x03788C),
            new HashRegion(0x568540, 0x035D2C),
            new HashRegion(0x59E270, 0x03788C),
            new HashRegion(0x5D5c10, 0x035D2C),
            new HashRegion(0x60B940, 0x03788C),
            new HashRegion(0x6432E0, 0x035D2C),
            new HashRegion(0x679010, 0x03788C),
            new HashRegion(0x6B09B0, 0x035D2C),
            new HashRegion(0x6E66E0, 0x03788C),
            new HashRegion(0x71E080, 0x035D2C),
            new HashRegion(0x753DB0, 0x03788C),
            new HashRegion(0x78B750, 0x035D2C),
            new HashRegion(0x7C1480, 0x03788C),
            new HashRegion(0x7F8E20, 0x035D2C),
            new HashRegion(0x82EB50, 0x03788C),
            new HashRegion(0x8663E0, 0x26899C)
        }),
        new Info(0xB05790, 0x1D7310, 0x1DF7E0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1d6d5c),
            new HashRegion(0x1d6e70, 0x323f2c),
            new HashRegion(0x4faeb0, 0x035d2c),
            new HashRegion(0x530be0, 0x03e5dc),
            new HashRegion(0x56f2d0, 0x035d2c),
            new HashRegion(0x5a5000, 0x03e5dc),
            new HashRegion(0x5e36f0, 0x035d2c),
            new HashRegion(0x619420, 0x03e5dc),
            new HashRegion(0x657b10, 0x035d2c),
            new HashRegion(0x68d840, 0x03e5dc),
            new HashRegion(0x6cbf30, 0x035d2c),
            new HashRegion(0x701c60, 0x03e5dc),
            new HashRegion(0x740350, 0x035d2c),
            new HashRegion(0x776080, 0x03e5dc),
            new HashRegion(0x7b4770, 0x035d2c),
            new HashRegion(0x7ea4a0, 0x03e5dc),
            new HashRegion(0x828b90, 0x035d2c),
            new HashRegion(0x85e8c0, 0x03e5dc),
            new HashRegion(0x89cea0, 0x2688ec)
        }),
        new Info(0xB20750, 0x1E2710, 0x1EABE0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1e215c),
            new HashRegion(0x1e2270, 0x323f6c),
            new HashRegion(0x5062f0, 0x03693c),
            new HashRegion(0x53cc30, 0x03f93c),
            new HashRegion(0x57c680, 0x03693c),
            new HashRegion(0x5b2fc0, 0x03f93c),
            new HashRegion(0x5f2a10, 0x03693c),
            new HashRegion(0x629350, 0x03f93c),
            new HashRegion(0x668da0, 0x03693c),
            new HashRegion(0x69f6e0, 0x03f93c),
            new HashRegion(0x6df130, 0x03693c),
            new HashRegion(0x715a70, 0x03f93c),
            new HashRegion(0x7554c0, 0x03693c),
            new HashRegion(0x78be00, 0x03f93c),
            new HashRegion(0x7cb850, 0x03693c),
            new HashRegion(0x802190, 0x03f93c),
            new HashRegion(0x841be0, 0x03693c),
            new HashRegion(0x878520, 0x03f93c),
            new HashRegion(0x8b7e60, 0x2688ec)
        }),
        new Info(0xB258E0, 0x1E2710, 0x1EABE0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1e215c),
            new HashRegion(0x1e2270, 0x32403c),
            new HashRegion(0x5063c0, 0x03693c),
            new HashRegion(0x53cd00, 0x04029c),
            new HashRegion(0x57d0b0, 0x03693c),
            new HashRegion(0x5b39f0, 0x04029c),
            new HashRegion(0x5f3da0, 0x03693c),
            new HashRegion(0x62a6e0, 0x04029c),
            new HashRegion(0x66aa90, 0x03693c),
            new HashRegion(0x6a13d0, 0x04029c),
            new HashRegion(0x6e1780, 0x03693c),
            new HashRegion(0x7180c0, 0x04029c),
            new HashRegion(0x758470, 0x03693c),
            new HashRegion(0x78edb0, 0x04029c),
            new HashRegion(0x7cf160, 0x03693c),
            new HashRegion(0x805aa0, 0x04029c),
            new HashRegion(0x845e50, 0x03693c),
            new HashRegion(0x87c790, 0x04029c),
            new HashRegion(0x8bca30, 0x268eac)
        }),
        new Info(0x849C30, 0x1E2710, 0x1EABE0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1e215c),
			new HashRegion(0x1e2270, 0x3221fc),
			new HashRegion(0x504580, 0x03693c),
			new HashRegion(0x53aec0, 0x02d6ec),
			new HashRegion(0x5686c0, 0x03693c),
			new HashRegion(0x59f000, 0x02d6ec),
			new HashRegion(0x5cc800, 0x03693c),
			new HashRegion(0x603140, 0x02d6ec),
			new HashRegion(0x630940, 0x03693c),
			new HashRegion(0x667280, 0x02d6ec),
			new HashRegion(0x694a80, 0x03693c),
			new HashRegion(0x6cb3c0, 0x02d6ec),
			new HashRegion(0x6f8bc0, 0x03693c),
			new HashRegion(0x72f500, 0x02d6ec),
			new HashRegion(0x75cd00, 0x03693c),
			new HashRegion(0x793640, 0x02d6ec),
			new HashRegion(0x7c0e40, 0x03693c),
			new HashRegion(0x7f7780, 0x02d6ec),
			new HashRegion(0x824e70, 0x024dbc)
        }),
        new Info(0x849C30, 0x1E2710, 0x1EABE0, 50, 50, new HashRegion[] {
            new HashRegion(0x000110, 0x1e215c),
            new HashRegion(0x1e2270, 0x3221fc),
            new HashRegion(0x504580, 0x03693c),
            new HashRegion(0x53aec0, 0x02d6ec),
            new HashRegion(0x5686c0, 0x03693c),
            new HashRegion(0x59f000, 0x02d6ec),
            new HashRegion(0x5cc800, 0x03693c),
            new HashRegion(0x603140, 0x02d6ec),
            new HashRegion(0x630940, 0x03693c),
            new HashRegion(0x667280, 0x02d6ec),
            new HashRegion(0x694a80, 0x03693c),
            new HashRegion(0x6cb3c0, 0x02d6ec),
            new HashRegion(0x6f8bc0, 0x03693c),
            new HashRegion(0x72f500, 0x02d6ec),
            new HashRegion(0x75cd00, 0x03693c),
            new HashRegion(0x793640, 0x02d6ec),
            new HashRegion(0x7c0e40, 0x03693c),
            new HashRegion(0x7f7780, 0x02d6ec),
            new HashRegion(0x824e70, 0x024dbc)
        }),
        new Info(0x86D560, 0x1E2710, 0x1F30B0, 100, 100, new HashRegion[] {
            new HashRegion(0x000110, 0x1e215c),
            new HashRegion(0x1e2270, 0x34582c),
            new HashRegion(0x527bb0, 0x03693c),
            new HashRegion(0x55e4f0, 0x02d70c),
            new HashRegion(0x58bd10, 0x03693c),
            new HashRegion(0x5c2650, 0x02d70c),
            new HashRegion(0x5efe70, 0x03693c),
            new HashRegion(0x6267b0, 0x02d70c),
            new HashRegion(0x653fd0, 0x03693c),
            new HashRegion(0x68a910, 0x02d70c),
            new HashRegion(0x6b8130, 0x03693c),
            new HashRegion(0x6eea70, 0x02d70c),
            new HashRegion(0x71c290, 0x03693c),
            new HashRegion(0x752bd0, 0x02d70c),
            new HashRegion(0x7803f0, 0x03693c),
            new HashRegion(0x7b6d30, 0x02d70c),
            new HashRegion(0x7e4550, 0x03693c),
            new HashRegion(0x81ae90, 0x02d70c),
            new HashRegion(0x8485a0, 0x024fbc)
        }),
        new Info(0x86D570, 0x1E2720, 0x1F30C0, 100, 100, new HashRegion[] {
            new HashRegion(0x000110, 0x1e216c),
            new HashRegion(0x1e2280, 0x34582c),
            new HashRegion(0x527bc0, 0x03693c),
            new HashRegion(0x55e500, 0x02d70c),
            new HashRegion(0x58bd20, 0x03693c),
            new HashRegion(0x5c2660, 0x02d70c),
            new HashRegion(0x5efe80, 0x03693c),
            new HashRegion(0x6267c0, 0x02d70c),
            new HashRegion(0x653fe0, 0x03693c),
            new HashRegion(0x68a920, 0x02d70c),
            new HashRegion(0x6b8140, 0x03693c),
            new HashRegion(0x6eea80, 0x02d70c),
            new HashRegion(0x71c2a0, 0x03693c),
            new HashRegion(0x752be0, 0x02d70c),
            new HashRegion(0x780400, 0x03693c),
            new HashRegion(0x7b6d40, 0x02d70c),
            new HashRegion(0x7e4560, 0x03693c),
            new HashRegion(0x81aea0, 0x02d70c),
            new HashRegion(0x8485b0, 0x024fbc)
        }),
        new Info(0x8F1BB0, 0x1E3968, 0x1F4308, 100, 100, new HashRegion[] {
            new HashRegion(0x000110, 0x1e339c),
            new HashRegion(0x1e34b0, 0x36406c),
            new HashRegion(0x547630, 0x03693c),
            new HashRegion(0x57df70, 0x033acc),
            new HashRegion(0x5b1b50, 0x03693c),
            new HashRegion(0x5e8490, 0x033acc),
            new HashRegion(0x61c070, 0x03693c),
            new HashRegion(0x6529b0, 0x033acc),
            new HashRegion(0x686590, 0x03693c),
            new HashRegion(0x6bced0, 0x033acc),
            new HashRegion(0x6f0ab0, 0x03693c),
            new HashRegion(0x7273f0, 0x033acc),
            new HashRegion(0x75afd0, 0x03693c),
            new HashRegion(0x791910, 0x033acc),
            new HashRegion(0x7c54f0, 0x03693c),
            new HashRegion(0x7fbe30, 0x033acc),
            new HashRegion(0x82fa10, 0x03693c),  
            new HashRegion(0x866350, 0x033acc),
            new HashRegion(0x899e20, 0x057d8c),
        })
    };

    public static Info GetInfo(byte[] headerBytes)
    {
        var major          = BitConverter.ToUInt32(headerBytes, 0);
        var minor          = BitConverter.ToUInt32(headerBytes, 4);
        var u1             = BitConverter.ToUInt16(headerBytes, 8);
        var headerRevision = BitConverter.ToUInt16(headerBytes, 10);
        var u2             = BitConverter.ToUInt16(headerBytes, 12);
        var saveRevision   = BitConverter.ToUInt16(headerBytes, 14);
        foreach (var header in Headers)
        {
            if (header.Check(major, minor, u1, headerRevision, u2, saveRevision))
                return Infos[header.SaveVersion];
        }
        return null;
    }

    public class Info
    {
        public Info(int size, int simpleDesignPatternOffset, int proDesignPatternOffset, int simpleDesignCount, int proDesignCount, HashRegion[] hashRegions)
        {
            this.Size = size;
            this.SimpleDesignPatternsOffset = simpleDesignPatternOffset;
            this.ProDesignPatternsOffset = proDesignPatternOffset;
            this.HashRegions = hashRegions;
            this.SimpleDesignCount = simpleDesignCount;
            this.ProDesignCount = proDesignCount;
        }

        public readonly int Size;
        public readonly int SimpleDesignPatternsOffset;
        public readonly int ProDesignPatternsOffset;
        public readonly int SimpleDesignCount;
        public readonly int ProDesignCount;

        public readonly HashRegion[] HashRegions;
    }
    public readonly struct HashRegion
    {
        public readonly int HashOffset;
        public readonly int BeginOffset;
        public readonly uint Size;

        public HashRegion(int hashOfs, int begOfs, uint size)
        {
            HashOffset = hashOfs;
            BeginOffset = begOfs;
            Size = size;
        }
        public HashRegion(int hashOfs, uint size)
        {
            HashOffset = hashOfs;
            BeginOffset = hashOfs + 0x04;
            Size = size;
        }
    }

    public class Header
	{
        public readonly uint Major;
        public readonly uint Minor;
        public readonly ushort HeaderRevision;
        public readonly ushort U1;
        public readonly ushort SaveRevision;
        public readonly ushort U2;
        public readonly string GameVersion;
        public readonly int SaveVersion;

        public Header(uint major, uint minor, ushort u1, ushort headerRevision, ushort u2, ushort saveRevision, string gameVersion, int saveVersion)
        {
            Major          = major;
            Minor          = minor;
            U1             = u1;
            HeaderRevision = headerRevision;
            U2             = u2;
            SaveRevision   = saveRevision;
            GameVersion    = gameVersion;
            SaveVersion    = saveVersion;
        }

        public bool Check(uint major, uint minor, ushort u1, ushort headerRevision, ushort u2, ushort saveRevision)
        {
            return Major == major && Minor == minor && U1 == u1 && HeaderRevision == headerRevision && U2 == u2 && SaveRevision == saveRevision;
        }
	}
}