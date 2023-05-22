using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace MyWorld
{
    public class WorldModify
    {
        public int x, y, z, to;
        public WorldModify(int a, int b, int c, int d)
        {
            x = a;
            y = b;
            z = c;
            to = d;
        }
    }

    public class World
    {
        // size
        public int[,,] worldCube = new int[50, 20, 50];
        public static string SAVE_PATH = "map/world.txt";

        public void modify(WorldModify p)
        {
            worldCube[p.x, p.y, p.z] = p.to;
        }

        public List<WorldModify> GetWorldModifyList()
        {
            List<WorldModify> res = new List<WorldModify>();
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 20; y++)
                {
                    for (int z = 0; z < 50; z++)
                    {
                        if (worldCube[x, y, z] != 0)
                        {
                            res.Add(new WorldModify(x, y, z, worldCube[x, y, z]));
                        }
                    }
                }
            }
            return res;
        }

        public void SaveWorld()
        {
            StreamWriter sw = new StreamWriter(SAVE_PATH);
            sw.Write(JsonConvert.SerializeObject(GetWorldModifyList()));
            sw.Flush();
            sw.Close();
        }

        /// <summary>
        /// read or create world
        /// </summary>
        public World(string path)
        {
            SAVE_PATH = path;
            if (!File.Exists(path))
            {
                // create a new
                for (int xx = 0; xx <= 40; xx++)
                {
                    for (int yy = 0; yy <= 7; yy++)
                    {
                        for (int zz = 0; zz <= 40; zz++)
                        {
                            if (yy == 7)
                                worldCube[xx, yy, zz] = 2;
                            else if (yy == 0)
                                worldCube[xx, yy, zz] = 4;
                            else
                                worldCube[xx, yy, zz] = 1;
                        }
                    }
                }

                SaveWorld();
            }
            else // read world
            {
                StreamReader sw = new StreamReader(path);
                string str = sw.ReadToEnd();
                List<WorldModify> all = JsonConvert.DeserializeObject<List<WorldModify>>(str);
                foreach (WorldModify p in all)
                {
                    worldCube[p.x, p.y, p.z] = p.to;
                }
                sw.Close();
            }
        }
    }
}
