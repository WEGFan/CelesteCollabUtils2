using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Celeste.Mod.CollabUtils2 {
    public class StrawberryCounterText : Entity
    {
        public string Author;
        public int CurrentStrawberryCount = -1;
        public int MaxStrawberries;
        public StrawberryCounterText(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            alpha = 0f;
            timer = 0f;
            widestCharacter = 0f;
            AddTag(Tags.HUD);
            AddTag(Tags.PauseUpdate);
            Add(textSfx = new SoundSource());
            Author = data.Attr("author", "JaThePlayer");
            MaxStrawberries = data.Int("maxBerries", 0);
            widestCharacter *= 0.9f;
        }

        public static string GetCounterName(string section) {
            return $"CollabUtils2_{section}_strawberries";
        }

        public static void IncrementCounter(string section) {
            string name = GetCounterName(section);
            (Engine.Scene as Level).Session.SetCounter(name, (Engine.Scene as Level).Session.GetCounter(name) + 1);
        }

        public static int GetCounter(string section) {
            string name = GetCounterName(section);
            if ((Engine.Scene as Level) != null)
                return (Engine.Scene as Level).Session.GetCounter(name);
            return -1;
        }

        public void CreateMessage()
        {
            CurrentStrawberryCount = GetCounter(Author);
            if (MaxStrawberries > 0)
            {
                message = $"{Author}\n{CurrentStrawberryCount}/{MaxStrawberries} Strawberries";
            } else
            {
                message = $"{Author}\nNo Strawberries";
            }
            
        }

        public override void Update()
        {
            if (CurrentStrawberryCount == -1)
            {
                CreateMessage();
                firstLineLength = CountToNewline(0);
                for (int i = 0; i < message.Length; i++)
                {
                    float x = ActiveFont.Measure(message[i]).X;
                    bool flag = x > widestCharacter;
                    if (flag)
                    {
                        widestCharacter = x;
                    }
                }
                widestCharacter *= 0.9f;
            }
            base.Update();
            bool paused = (Scene as Level).Paused;
            if (paused)
            {
                textSfx.Pause();
            }
            else
            {
                timer += Engine.DeltaTime;
                bool flag = !Show;
                if (flag)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime);
                    bool flag2 = alpha <= 0f;
                    if (flag2)
                    {
                        index = firstLineLength;
                    }
                }
                else
                {
                    alpha = Calc.Approach(alpha, 1f, Engine.DeltaTime * 2f);
                    bool flag3 = alpha >= 1f;
                    if (flag3)
                    {
                        index = Calc.Approach(index, message.Length, 32f * Engine.DeltaTime);
                    }
                }
                bool flag4 = Show && alpha >= 1f && index < message.Length;
                if (flag4)
                {
                    bool flag5 = !textSfxPlaying;
                    if (flag5)
                    {
                        textSfxPlaying = true;
                        textSfx.Play(Dreamy ? "event:/ui/game/memorial_dream_text_loop" : "event:/ui/game/memorial_text_loop", null, 0f);
                        textSfx.Param("end", 0f);
                    }
                }
                else
                {
                    bool flag6 = textSfxPlaying;
                    if (flag6)
                    {
                        textSfxPlaying = false;
                        textSfx.Stop(true);
                        textSfx.Param("end", 1f);
                    }
                }
                textSfx.Resume();
            }
        }

        private int CountToNewline(int start)
        {
            int i;
            for (i = start; i < message.Length; i++)
            {
                bool flag = message[i] == '\n';
                if (flag)
                {
                    break;
                }
            }
            return i - start;
        }

        public override void Render()
        {
            bool flag = (Scene as Level).FrozenOrPaused || (Scene as Level).Completed;
            if (!flag)
            {
                bool flag2 = index > 0f && alpha > 0f;
                if (flag2)
                {
                    Camera camera = SceneAs<Level>().Camera;
                    // -350f
                    Vector2 vector = new Vector2((Position.X - camera.X) * 6f, (Position.Y - camera.Y) * 6f - ActiveFont.LineHeight * 3.3f);
                    bool flag3 = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
                    if (flag3)
                    {
                        vector.X = 1920f - vector.X;
                    }
                    float num = Ease.CubeInOut(alpha);
                    int num2 = (int)Math.Min(message.Length, index);
                    int num3 = 0;
                    float num4 = 64f * (1f - num);
                    int num5 = CountToNewline(0);
                    for (int i = 0; i < num2; i++)
                    {
                        char c = message[i];
                        bool flag4 = c == '\n';
                        if (flag4)
                        {
                            num3 = 0;
                            num5 = CountToNewline(i + 1);
                            num4 += ActiveFont.LineHeight * 1.1f;
                        }
                        else
                        {
                            float x = 1f;
                            float x2 = -num5 * widestCharacter / 2f + (num3 + 0.5f) * widestCharacter;
                            float num6 = 0f;
                            bool flag5 = Dreamy && c != ' ' && c != '-' && c != '\n';
                            if (flag5)
                            {
                                c = message[(i + (int)(Math.Sin((timer * 2f + i / 8f)) * 4.0) + message.Length) % message.Length];
                                num6 = (float)Math.Sin((timer * 2f + i / 8f)) * 8f;
                                x = ((Math.Sin((timer * 4f + i / 16f)) < 0.0) ? -1 : 1);
                            }
                            ActiveFont.Draw(c, vector + new Vector2(x2, num4 + num6), new Vector2(0.5f, 1f), new Vector2(x, 1f), Color.White * num);
                            num3++;
                        }
                    }
                }
            }
        }

        public bool Show = true;

        public bool Dreamy = false;

        private float index;

        private string message;

        private float alpha;

        private float timer;

        private float widestCharacter;

        private int firstLineLength;

        private SoundSource textSfx;

        private bool textSfxPlaying;
    }
}
