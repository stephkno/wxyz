using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using UIKit;
using Google.MobileAds;

/*
 * TO DO LIST:
 * x-Colors/Levels
 * -GameModes:
 * --Arcade
 * --60 Sec
 * --Freeplay (premium)
 * -Splash Screen
 * x-GameOver Screen
 * x-Highscore(local)
 * -IAP Premium/NoAds
 * ~~~~~~~~~~~~~~~~~~
 * -Power Plays:
 * -Bombs
 * -Mysteries
 * 
 * -In Game Currency
 * -High Score (leaderboards)
 * -Slot Machine/Gumball Machine Reward Minigame
 * -
 *
*/

namespace wxyz
{
    
    public class Game1 : Game
    {

        Boolean isPlaying = false;
        //Game Modes:
        //0 - arcade
        //AdMob
        BannerView bannerView;
        //UI/Sprite Definitions
        //utils
        TouchCollection touchCollection;
        SpriteBatch spriteBatch;
        RenderTarget2D renderTarget;
        GraphicsDeviceManager graphics;
        int minSwipeHeight;
        Boolean isOnscreen;
        Boolean renderTargetEnabled = true;

        //sprites
        public Texture2D box;
        public Texture2D tileSprite;
        public Texture2D foreground;
        public Texture2D bar;
        public Texture2D tilespriteSelected;
        public Texture2D pointer;
        public Texture2D gauge;
        public Texture2D background;
        public Texture2D tap;
        public Texture2D play;
        public Texture2D eye;
        public Texture2D bomb;


        //floating texts display score points
        public List<floatingText> floatingTexts = new List<floatingText>();

        //admob values
        String bannerAdUnitID = "ca-app-pub-6507479540703114/4547114013";//"ca-app-pub-3940256099942544/6300978111";;
        String interstitialAdUnitID = "ca-app-pub-6507479540703114/3388737130";//"ca-app-pub-3940256099942544/1033173712";

        List<SoundEffect> soundEffects = new List<SoundEffect>();

        string SAVEFILENAME = "file";
        IsolatedStorageFile savegameStorage;

        Boolean skipMoved = false;

        Boolean gameOverSplash = true;
        Boolean isFree;

        //game stats
        int maxWordLength = 9;
        int tutorial = 0;
        int level = 0;
        int score = 0;
        int highScore = 0;
        int levelCount = 25;
        int dispScore = 0;
        int dispHighScore = 0;
        int barSize;
        int displayType;
        float highscoreOffset = 0.0f;
        float waveGenerator = 0.0f;
        Boolean newHighScore = false;

        //graphics points
        Vector2 barPos;
        Vector2 initPosition;
        Vector2 tapPos;
        Vector2 fontOffset;

        //world vars
        Tile[,] world = new Tile[5, 5];
        private int width = 5;
        private int height = 5;
        private Rectangle bounds;
        private int tileSize;
        Vector2 offset;
        float buttonTextOffset = 0.0f;
        Texture2D pixel; 

        //fonts
        SpriteFont font;
        SpriteFont smallfont;
        SpriteFont tinyfont;
        SpriteFont barfont;
        //time
        String[] time = new String[2];

        Clock clock = new Clock();
        Color foregroundColor = Color.White;
        Color bgcolor = Color.WhiteSmoke;
        Color highScoreColor = Color.Black;

        //Color Scheme
        Color[] colors = new Color[20]{

            new Color(26, 188, 156),
            new Color(46, 204, 113),
            new Color(52, 152, 219),
            new Color(155, 89, 182),
            new Color(52, 73, 94),

            new Color(22, 160, 133),
            new Color(39, 174, 96),
            new Color(41, 128, 185),
            new Color(142, 68, 173),
            new Color(44, 62, 80),

            new Color(241, 196, 15),
            new Color(230, 126, 34),
            new Color(231, 76, 60),
            new Color(206, 210, 211),
            new Color(149, 165, 166),

            new Color(243, 156, 18),
            new Color(211, 84, 0),
            new Color(192, 57, 43),
            new Color(189, 195, 199),
            new Color(127, 140, 141)

        };
        int numColors = 20;

        //alphabet
        List<char> alphabet = new List<char>();
        char[] alphabetMaster = new char[26]{
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };
        /*char[] alphabetMaster = new char[26]{
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };*/

        string vowels = "aeiou";

        //Random object
        readonly Random r = new Random(DateTime.Now.Millisecond);

        //Object Vectors
        List<Tile> tiles = new List<Tile>();
        List<Tile> word = new List<Tile>();
        List<String> words = new List<String>();
        List<String> completedWords = new List<String>();

        UIViewController viewController;
        Interstitial interstitial;
        //Game Functions
        //Initialize Game Vars
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
        }

        protected override void Initialize()
        {
            //AdMob initialize config, add view, load request
            bannerView = new BannerView(AdSizeCons.Banner)
            {
                AdUnitID = bannerAdUnitID,
                RootViewController = viewController
            };

            level = r.Next(0, numColors);

            viewController = (UIViewController)this.Services.GetService(typeof(UIViewController));

            savegameStorage = IsolatedStorageFile.GetUserStoreForApplication();

            // open isolated storage, and write the savefile.
            if (savegameStorage.FileExists(SAVEFILENAME))
            {
                IsolatedStorageFileStream fs = null;
                try
                {
                    fs = savegameStorage.OpenFile(SAVEFILENAME, System.IO.FileMode.Open);
                }
                catch (IsolatedStorageException e)
                {
                    // The file couldn't be opened, even though it's there.
                    Console.WriteLine("Isolated Storage Exception:");
                    Console.WriteLine(e);
                }

                if (fs != null)
                {
                    // Reload the last state of the game.  This consists of the
                    // highest score and first time launch tutorial mode boolean

                    byte[] saveBytes = new byte[256];
                    int count = fs.Read(saveBytes, 0, 256);
                    if (count > 0)
                    {
                        // the first byte is the mode, rest is the string.
                        //Console.WriteLine(saveBytes[1]);
                        highScore = saveBytes[0];
                        fs.Close();
                    }
                }
            }
            else
            {
                Console.WriteLine("Tutorial");
                tutorial = 1;
            }

            //render target parameters
            renderTarget = new RenderTarget2D(
                GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight
            );

            //add banner ad view controller
            viewController.Add(bannerView);


            bannerView.LoadRequest(Request.GetDefaultRequest());
            loadInterstitial();


        // Make a 1x1 texture named pixel.  
            pixel = new Texture2D(GraphicsDevice, 1, 1);

            // Create a 1D array of color data to fill the pixel texture with.  
            Color[] colorData = {
                Color.White,
                    };

            // Set the texture data with our color information.  
            pixel.SetData<Color>(colorData);

            base.Initialize();

        }
        void loadInterstitial()
        {
            interstitial = new Interstitial(interstitialAdUnitID);
            interstitial.LoadRequest(Request.GetDefaultRequest());
        }
        //Load Game Content
        protected override void LoadContent()
        {
            //load sounds
/*            soundEffects.Add(Content.Load<SoundEffect>("Content/click"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/click2"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/error"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/hint"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/ping"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/waterdrop"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/win"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/wrong"));
            soundEffects.Add(Content.Load<SoundEffect>("Content/shatter"));
*/
            //Load words
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader("words");
            while ((line = file.ReadLine()) != null)
            {
                words.Add(line);
            }

            file.Close();
            //reset alphabet
            resetAlphabet();

            Console.WriteLine(words.Count + " words added");

            //load graphics and font
            font = Content.Load<SpriteFont>("Content/MaxFont");
            smallfont = Content.Load<SpriteFont>("Content/MidFont");
            tinyfont = Content.Load<SpriteFont>("Content/MinFont");

            //load graphics
            // tilespriteSelected = Content.Load<Texture2D>("Content/tile_selected");
            bar = Content.Load<Texture2D>("Content/bar");
            tileSprite = Content.Load<Texture2D>("Content/tile");
            pointer = Content.Load<Texture2D>("Content/pointer");
            gauge = Content.Load<Texture2D>("Content/bomb");
            background = Content.Load<Texture2D>("Content/foreground");
            tap = Content.Load<Texture2D>("Content/tap");
            play = Content.Load<Texture2D>("Content/play");
            //eye = Content.Load<Texture2D>("Content/eye");
            foreground = Content.Load<Texture2D>("Content/foreground");
            bomb = Content.Load<Texture2D>("Content/bomb");


            //graphics sizes
            bounds = GraphicsDevice.Viewport.Bounds;
            Console.WriteLine(bounds);
            if (bounds.Width == 640)
            {
                //iPhone 5s,SE
                displayType = 0;
                height = 5;

            }
            else if (bounds.Width == 750)
            {
                //iPhone 8,7,6
                displayType = 1;
                height = 5;

            }
            else if (bounds.Width == 1125)
            {
                //iPhone X
                displayType = 2;
                height = 5;

            }
            else if (bounds.Width == 1242)
            {
                //iPhone 8,7,6 Plus
                displayType = 3;
                height = 5;
            }
            Console.WriteLine(displayType);
            if (displayType == 0)
            {
                //se, 5s
                bannerView.Frame = new CoreGraphics.CGRect(0, bounds.Height / 2 - 50, 320, 50);
                barSize = 100;
                barfont = smallfont;
                barPos = new Vector2(0, bounds.Height - 225);
                offset.Y = 500;
                offset.X = 136;
                buttonTextOffset = -10.0f;
                maxWordLength = 8;
                minSwipeHeight = 895;
                fontOffset = new Vector2(-0, -0);

            }
            else if (displayType == 1)
            {
                //6, 7, 8
                bannerView.Frame = new CoreGraphics.CGRect(bounds.Width / 4 - 160, bounds.Height / 2 - 50, 320, 50);
                barSize = 100;
                barfont = smallfont;
                barPos = new Vector2(0, bounds.Height - 340);
                offset.Y = 255;
                offset.X = 160;
                buttonTextOffset = -15.0f;
                fontOffset = new Vector2(-10, -0);
                minSwipeHeight = 1000;

            }
            else if (displayType == 2)
            {
                //X
                bannerView.Frame = new CoreGraphics.CGRect(bounds.Width / 4 - 160, bounds.Height / 2 - 50, 320, 50);
                barSize = 200;
                barfont = font;
                barPos = new Vector2(0, bounds.Height - 500);
                offset.Y = -150;
                offset.X = 170;
                fontOffset = new Vector2(-0, 0);

                minSwipeHeight = 1800;


            }
            else if (displayType == 3)
            {
                //6, 7, 8+
                bannerView.Frame = new CoreGraphics.CGRect((bounds.Width / 3) / 2 - 160, bounds.Height / 3 - 50, 320, 50);
                barSize = 200;
                barfont = font;
                barPos = new Vector2(0, bounds.Height - 420);
                offset.Y = -25;
                offset.X = 200;
                minSwipeHeight = 1600;
                fontOffset = new Vector2(-0, -0);

            }

            clock.setBounds(bounds);
            Console.WriteLine("Display: " + displayType);

            tileSize = (bounds.Width) / width - 8;
            //init sprite batch object
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //initialize game world
            setupWorld();
        }
        protected override void OnExiting(object sender, System.EventArgs args)
        {
            base.OnExiting(sender, args);
        }

        void setupWorld()
        {

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //Create new tile
                    Tile t = new Tile(pixel, bomb, tilespriteSelected, tileSize);
                    //Configure tile
                    int a = x * tileSize;
                    int b = y * tileSize;
                    //set position
                    Vector2 n = new Vector2(a, b);
                    t.setPosition(n+offset);
                    //set home position
                    t.setHome(t.getPosition());
                    //set scale
                    t.setTileSize(tileSize-3);
                    //set tile letter
                    char l = alphabet[0];
                    alphabet.RemoveAt(0);
                    t.setLetter(l);
                    //if alphabet list is too small, reset alphabet
                    if (alphabet.Count < 5)
                    {
                        resetAlphabet();
                    }
                    //set bomb tile?
                    if (r.Next(0, 27) > 26)
                    {
                        t.setTile(box);
                    }
                    //change consonant/vowel color
                    if (vowels.Contains(t.getLetter().ToString()))
                    {
                        t.setColor(colors[level % numColors]);
                    }
                    else
                    {
                        t.setColor(colors[level % numColors]);
                    }
                    //put tile in world
                    world[y, x] = t;
                    //for tutorial mode
                    if (tutorial == 1)
                    {
                        tapPos = new Vector2(bounds.Width / 2, bounds.Height / 2);
                    }
                }
            }
            //reset score
            score = 0;
            //reset clock
            clock.reset();
            clock.setTicking(false);
        }
        //map function maps two values
        float Map(float v, float fromSource, float toSource, float fromTarget, float toTarget)
        {
            return (v - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
        }
        //function resets alphabet
        void resetAlphabet()
        {
            for (int i = 0; i < 8; i++)
            {
                string w = words[r.Next(0, words.Count - 1)];

                foreach (char c in w)
                {
                    alphabet.Add(c);
                }
            }
        }
        //function to submit word
        void submitWord()
        {

            string checkword = word.ToString();
            int scoreCount = 0;

            foreach (Tile t in word)
            {
                scoreCount++;
                floatingText text = new floatingText(new Vector2(offset.X, offset.Y), new Vector2(t.getPosition().X + 80, t.getPosition().Y), "+1", Color.Red, spriteBatch, smallfont);
                floatingTexts.Add(text);

                if (t.getBomb())
                {
                    char testchar = t.getLetter();
                    foreach (Tile a in world)
                    {
                        if (a == null)
                        {
                            continue;
                        }
                        if (testchar == a.getLetter())
                        {

                        }
                    }
                }
            }

            score += scoreCount;
            if (score > highScore)
            {
                highScore = score;

                newHighScore = true;

                IsolatedStorageFileStream fs = null;
                fs = savegameStorage.OpenFile(SAVEFILENAME, System.IO.FileMode.Create);
                if (fs != null)
                {
                    // just overwrite the existing info for this example.
                    fs.WriteByte((byte)highScore);
                    fs.Close();
                }

            }
            completedWords.Add(checkword);

            Console.WriteLine(checkword.ToString());

            //reset word and return tiles to board
            word.RemoveRange(0, word.Count);
            level++;

            if (completedWords.Count > levelCount)
            {
                levelCount *= 2;
                clock.increaseSpeed();
            }
            //for tutorial mode
            if (tutorial == 3)
            {
                tutorial = 4;
                tapPos = new Vector2(1000, 200);
            }
            //update tile world
            updateWorld();
        }
        //change tile colors
        public void changeColors()
        {
            foreach (Tile t in world)
            {
                if (t == null)
                {
                    continue;
                }
                if (vowels.Contains(t.getLetter().ToString()))
                {
                    t.setColor(colors[level % numColors]);
                }
                else
                {
                    t.setColor(colors[level % numColors]);
                }

            }
        }
        //check word
        Boolean checkWord(List<wxyz.Tile> list)
        {

            string checkword = "";

            foreach (Tile t in list)
            {
                checkword += t.getLetter();
            }

            checkword = checkword.ToLower();

            if (words.Contains(checkword))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        //update world
        void updateWorld()
        {

            int ycount;

            for (int y = height - 2; y >= 0; y--)
            {
                for (int x = 0; x < width; x++)
                {
                    ycount = 0;
                    if (!world[y, x].getSelected())
                    {
                        while (y + ycount + 1 < height && world[y + ycount + 1, x].getSelected() && y + ycount < height - 1)
                        {
                            ycount++;
                        }
                        if (ycount > 0)
                        {
                            Vector2 b = world[y, x].getPosition();
                            Tile tmp = world[y + ycount, x];

                            world[y, x].move(ycount);//this is our origin tile, it should be moving down
                            world[y + ycount, x].move(-ycount);//this is our displaced tile, it should move up

                            world[y + ycount, x] = world[y, x];
                            world[y, x] = tmp;
                        }
                    }

                }
            }

            //reset floater tiles
            foreach (Tile t in world)
            {

                if (t.getSelected())
                {
                    Console.WriteLine("A"); 
                    //if tile is bomb, reset
                    if (t.getBomb())
                    {
                        t.setBomb(false);
                    }
                    //if tile is not bomb, decide if to make it one
                    int randomRationalNumber = 16 / r.Next(1, 10);

                    if (randomRationalNumber > 15)
                    {
                        t.setBomb(true);
                    }

                    t.setSelected(false);
                    t.setPosition(new Vector2(t.getPosition().X, t.getPosition().Y - tileSize * height));
                    t.goHome();

                    char l = alphabet[0];

                    alphabet.RemoveAt(0);

                    if (alphabet.Count < 5)
                    {
                        resetAlphabet();
                    }
                    t.setLetter(l);

                    if (vowels.Contains(t.getLetter().ToString()))
                    {
                        t.setColor(colors[level % numColors]);
                    }
                    else
                    {
                        t.setColor(colors[level % numColors]);
                    }

                    if (t.getBomb())
                    {
                        char v = vowels[r.Next(0, vowels.Length - 1)];
                        t.setLetter(v);
                        Console.WriteLine("Set a bomb: " + v);
                    }
                }
            }
        }
        //Update Game
        protected override void Update(GameTime gameTime)
        {
            //get system clock time
            time[0] = DateTime.Now.ToString("h:mm:ss tt");
            //update time w/o ms
            if (time[1] != time[0])
            {
                time[1] = time[0];
                Console.WriteLine(time[1]);
            }

            //update wave generator
            waveGenerator += 0.05f;

            foreach (Tile t in world)
            {
                isOnscreen = t.getIsOnScreen();
                if (isOnscreen)
                {
                    break;
                }
            }

            if (!isOnscreen && gameOverSplash)
            {

                Console.WriteLine("Ad and reset tiles");

                if (interstitial.IsReady)
                {
                    interstitial.PresentFromRootViewController(viewController);
                }
                else
                {
                    Console.WriteLine("Ad wasn't ready...");
                }
                // menu.moveTo(new Vector2(0, 0), false);
                foreach (Tile t in world)
                {
                    t.setIsOnScreen(true);
                }

                loadInterstitial();
                gameOverSplash = true;

            }


            if (!isPlaying)
            {
                if (score < dispScore)
                {
                    dispScore--;
                }
                if (dispHighScore != highScore)
                {
                    dispHighScore = highScore;
                }
            }
            if (isPlaying)
            {
                //update score
                if (score > dispScore)
                {
                    dispScore++;
                }
                if (highScore > dispHighScore)
                {
                    dispHighScore++;
                }
                if (tutorial == 0)
                {
                    if (clock.update())
                    {
                        //update clock

                    }
                    else
                    {
                        //game over!
                        if (tutorial > 0)
                        {
                            tapPos = new Vector2(65, 500);
                            tutorial = 1;
                        }
                        isPlaying = false;
                       // soundEffects[soundEffects.Count - 2].Play();
                       // soundEffects[soundEffects.Count - 1].Play();

                        //menuString = "New High Score!";


                        clock.setTicking(false);
                        word.Clear();
                        if (score > highScore)
                        {
                            highScore = score;
                        }
                        score = 0;
                        level = 19;

                        gameOverSplash = true;

                        foreach (String w in completedWords)
                        {
                            //   Console.Write(w);
                        }


                        foreach (Tile t in world)
                        {
                            if (t == null)
                            {
                                continue;
                            }
                            t.popOut(new Vector2(r.Next(-10, 10), r.Next(-10, 0)));
                        }

                    }
                }
            }

            //update tile physics

            foreach (Tile t in world)
            {
                if (t != null)
                {
                    t.updateTile();
                }
            }

            touchCollection = TouchPanel.GetState();
            //calculate FPS
            float frameRate = 1 / (float)gameTime.ElapsedGameTime.TotalSeconds;

            //Touch Input
            foreach (TouchLocation touch in touchCollection)
            {
                //get position
                int x = (int)touch.Position.X;
                int y = (int)touch.Position.Y;
                Point p;
                p.X = x + tileSize;// - (int)offset.X;
                p.Y = y + tileSize;// - (int)offset.Y;
                //if pressed
                if (touch.State == TouchLocationState.Pressed)
                {
                    Console.WriteLine(touch.Position);
                    if (tutorial == 4)
                    {
                        tutorial = 0;
                    }
                    //save init position for gesture recognizer
                    initPosition = touch.Position;

                    //Test if game has been initiated
                    if (isPlaying)
                    {
                        foreach (Tile t in world) 
                        {
                            if (t == null)
                            {
                                continue;
                            }
                            Rectangle rect = new Rectangle((int)t.getPosition().X, (int)t.getPosition().Y, tileSize, tileSize);

                            //if touched in tile

                            if (rect.Contains(p))
                            {

                                if (!t.getSelected())
                                {
                                    if (tutorial == 1)
                                    {
                                        tutorial++;
                                    }
                                    //if word is small enough and tile is active
                                    if (word.Count < maxWordLength)
                                    {
                                        //when user clicks on tile: add tile to word, hide tile
                                        t.setSelected(true);
                                        word.Add(t);
                                        int a = word.Count;
                                        initPosition = touch.Position;
                       //                 soundEffects[1].Play();

                                        break;
                                    }
                                }
                                else
                                {
                                    t.setSelected(false);
                                //    soundEffects[0].Play();
                                    word.Remove(t);
                                    Console.WriteLine("ABC");
                                    skipMoved = true;
                                }
                            }


                        }
                    }

                }
                //drag tile
                if (touch.State == TouchLocationState.Moved)
                {
                    //Console.WriteLine(skipMoved);
                    if (isPlaying && !skipMoved)
                    {
                        foreach (Tile t in world)
                        {

                            if (t == null)
                            {
                                continue;
                            }

                            Rectangle rect = new Rectangle((int)t.getPosition().X, (int)t.getPosition().Y, tileSize, tileSize);

                            //if touched in tile
                            if (rect.Contains(p))
                            {
                                if (!t.getSelected())
                                {

                                    //if word is small enough and tile is active
                                    if (word.Count < maxWordLength)
                                    {
                                        //when user clicks on tile: add tile to word, hide tile
                                        t.setSelected(true);
                                        word.Add(t);
                                        int a = word.Count;
                                        initPosition = touch.Position;
                                       // soundEffects[1].Play();

                                        break;
                                    }
                                }
                                else
                                {
                                    ;//for if user touches anything other than tiles
                                }
                            }
                        }
                    }
                }
            }
            //for each touch
            foreach (TouchLocation touch in touchCollection)
            {

                //when touch is released
                if (touch.State == TouchLocationState.Released)
                {

                    if (touch.Position.Y > minSwipeHeight)
                    {
                        if (checkWord(word))
                        {

                            clock.setBack((float)word.Count * 0.80f);
                            submitWord();
                            for (int y = 0; y < Math.Sqrt(world.Length); y++){
                                for (int x = 0; x < Math.Sqrt(world.Length); x++)
                                {
                                    Console.Write(world[y, x].getLetter() + " ");
                                }
                                Console.WriteLine();
                            }
                            //soundEffects[4].Play();
                            // Trigger feedback

                        }
                        else
                        {
                            //word not found
                            if (word.Count > 0)
                            {
                                //soundEffects[2].Play();
                            }
                        }
                    }
                    if (skipMoved)
                    {
                        skipMoved = false;
                    }
                    foreach (Tile t in world)
                    {
                        if (t == null)
                        {
                            continue;
                        }
                        isFree = t.isFree();
                        if (isFree)
                        {
                            break;
                        }
                    }

                    if (!isFree && !isPlaying)
                    {
                        gameOverSplash = false;
                      //  soundEffects[3].Play();

                        newHighScore = false;
                        highscoreOffset = 0.0f;
                        clock.reset();
                        clock.setTicking(true);
                        isPlaying = true;

                        changeColors();

                        foreach (Tile t in world)
                        {
                            if (t == null)
                            {
                                continue;
                            }

                            char l = alphabet[0];

                            alphabet.RemoveAt(0);

                            if (alphabet.Count < 5)
                            {
                                resetAlphabet();
                            }

                            t.setLetter(l);
                            t.setSelected(false);
                            t.setPosition(new Vector2(t.getPosition().X, t.getPosition().Y - (tileSize * height)));
                            t.setVisibility(true);
                            t.goHome();
                        }

                    }
                    else
                    {
                        //find drag difference

                        Vector2 delta = touch.Position - initPosition;
                        if (Math.Abs(delta.X) > 50)
                        {
                            //if touched below game level
                            if (touch.Position.Y > minSwipeHeight)
                            {
                                if (Math.Abs(delta.X) > Math.Abs(delta.Y))
                                {
                                    //Swipe Right
                                    if (delta.X > 30)
                                    {

                                        Console.WriteLine("Right");
                                        //Check word

                                        if (checkWord(word))
                                        {

                                            clock.setBack((float)word.Count * 0.80f);
                                            submitWord();
                                           // soundEffects[4].Play();
                                            // Trigger feedback

                                        }
                                        else
                                        {
                                            //word not found
                                            if (word.Count > 0)
                                            {
                                              //  soundEffects[2].Play();
                                            }
                                        }

                                    }
                                    //Swipe Left
                                    else
                                    {
                                        Console.WriteLine("Left");
                                        if (word.Count > 0)
                                        {
                                           // soundEffects[0].Play();
                                            word[word.Count - 1].setSelected(false);
                                            word.RemoveAt(word.Count - 1);
                                        }
                                    }
                                }
                            }
                            //Swipe up/down
                            else
                            {
                                if (delta.Y > 0)
                                {
                                    Console.WriteLine("Down");
                                }
                                else
                                {
                                    Console.WriteLine("Up");
                                }
                            }
                        }
                        else
                        {
                            //if touched on clear button

                            Rectangle clearButton = new Rectangle(bounds.Width - 150, (int)barPos.Y, 150, 150);
                            if (clearButton.Contains(touch.Position))
                            {
                                if (!checkWord(word))
                                {
                                    //soundEffects[0].Play();
                                    foreach (Tile t in world)
                                    {
                                        if (t.getSelected())
                                        {
                                            t.setSelected(false);
                                        }
                                    }
                                    word.Clear();

                                }
                                else
                                {

                                }
                            }
                        }
                    }
                }
            }
            foreach (floatingText f in floatingTexts)
            {

                if (f.updateText())
                {

                }
                else
                {
                    floatingTexts.Remove(f);
                    break;
                }


            }
            base.Update(gameTime);
        }
        //Draw
        protected override void Draw(GameTime gameTime)
        {

            //set render target
            if (renderTargetEnabled) graphics.GraphicsDevice.SetRenderTarget(renderTarget);

            //clear screen draw background
            GraphicsDevice.Clear(bgcolor);
            //begin drawing sprites1
            spriteBatch.Begin();
            //draw world foreground
            spriteBatch.Draw(foreground, new Rectangle(10, 100, tileSize * width + 25, tileSize * (height) + 30), foregroundColor);
            //display FPS - debug
            //spriteBatch.DrawString(font, fps, new Vector2(0, 0), Color.White);

            if (gameOverSplash)
            {
                //spriteBatch.DrawString(smallfont, "wxyz", new Vector2(bounds.Width / 2 - smallfont.MeasureString("wxyz").X/2, 200), Color.White);
                spriteBatch.Draw(play, new Vector2(bounds.Width / 2 - play.Width / 2, bounds.Height / 2 - play.Height / 2), Color.Black);
            }

            //for each tile
            foreach (Tile t in world)
            {
                if (t == null)
                {
                    continue;
                }
                //get tile pos
                Vector2 p = t.getPosition();
                //if tile is active
                float a = p.X + offset.X;
                float b = p.Y + offset.Y;
                Boolean drawText;

                if (clock.getTicking())
                {
                    drawText = true;
                }
                else
                {
                    drawText = true;
                }

                if (displayType == 0)
                {
                    t.drawTile(drawText, font, spriteBatch, fontOffset);
                }
                else if (displayType == 1)
                {
                    t.drawTile(drawText, font, spriteBatch, fontOffset);
                }
                else if (displayType == 2)
                {
                    t.drawTile(drawText, font, spriteBatch, fontOffset);

                }
                else if (displayType == 3)
                {
                    t.drawTile(drawText, font, spriteBatch, fontOffset);
                }

            }
            //display word
            string printword = "";
            foreach (Tile w in word)
            {
                printword += w.getLetter().ToString();
            }

            //bar background
            Color barColor;
            String buttonText;
            if (word.Count > 0)
            {
                //check word
                if (checkWord(word))
                {
                    //word is valid
                    if (tutorial == 2)
                    {
                        tapPos = new Vector2(1030, 1800);
                        tutorial = 3;
                    }

                    barColor = Color.Green * 0.5f;
                    buttonText = "o";
                }
                else
                {
                    if (tutorial == 3)
                    {
                        tutorial = 2;
                    }
                    //word is invalid
                    barColor = Color.DarkGray * 0.5f;
                    buttonText = "x";
                }
            }
            else
            {
                //set bar color gray, no word
                barColor = Color.DarkGray * 0.5f;
                buttonText = "";

            }

            //draw bar
            if (isPlaying)
            {
                spriteBatch.Draw(bar, new Rectangle(0, (int)barPos.Y, bounds.Width, barSize), barColor);
                //draw clear button
                spriteBatch.DrawString(tinyfont, buttonText, new Vector2(bounds.Width - tinyfont.MeasureString("x").X * 1.4f - buttonTextOffset, barPos.Y - 25), Color.White);

                //draw word
                spriteBatch.DrawString(barfont, printword, new Vector2(bounds.Width / 2 - barfont.MeasureString(printword).X / 2 - 40, barPos.Y - 23), Color.White);

            }

            clock.draw(gauge, pointer, spriteBatch, new Vector2(bounds.Width - 125, 125));

            foreach (floatingText f in floatingTexts)
            {
                f.drawText();
            }
            if (isPlaying)
            {
                //draw tutorial
                if (tutorial == 1 || tutorial == 3 || tutorial == 4)
                {
                    spriteBatch.Draw(tap, new Vector2(tapPos.X, tapPos.Y + (float)Math.Sin(waveGenerator) * 10), Color.White);

                    if (tutorial == 1)
                    {
                        // spriteBatch.DrawString(tinyfont, "Tap letters to make words", new Vector2(tapPos.X + 10, tapPos.Y - 200), Color.White);
                    }
                    if (tutorial == 3)
                    {
                        // spriteBatch.DrawString(tinyfont, "Swipe here to submit", new Vector2(tapPos.X - 800, tapPos.Y - 120), Color.White);
                    }
                    if (tutorial == 4)
                    {
                        // spriteBatch.DrawString(tinyfont, "Time runs out quickly", new Vector2(tapPos.X - 800, tapPos.Y + 65), Color.White);
                    }
                }
            }
            //draw menu
            if (newHighScore)
            {

                highscoreOffset = (float)Math.Sin(waveGenerator) * 10;
            }
            else
            {
                highscoreOffset = 0.0f;
            }
            //draw score
            if (displayType == 0)
            {

                //5s
                //spriteBatch.DrawString(tinyfont, dispHighScore.ToString(), new Vector2(20, 150 + highscoreOffset), highScoreColor);

                //spriteBatch.DrawString(smallfont, dispScore.ToString(), new Vector2(20, 25), highScoreColor);
            }
            if (displayType == 1)
            {
                //8
                //spriteBatch.DrawString(tinyfont, dispHighScore.ToString(), new Vector2(20, 150 + highscoreOffset), highScoreColor);

                //spriteBatch.DrawString(smallfont, dispScore.ToString(), new Vector2(20, 25), highScoreColor);
            }
            if (displayType == 2)
            {
                //X
                // spriteBatch.DrawString(font, dispScore.ToString(), new Vector2(20, 20), highScoreColor);
                //spriteBatch.DrawString(font, dispHighScore.ToString(), new Vector2(20, 200 + highscoreOffset), highScoreColor);

            }
            if (displayType == 3)
            {
                //8+
                //spriteBatch.DrawString(smallfont, dispHighScore.ToString(), new Vector2(30, 200 + highscoreOffset), highScoreColor);

                //spriteBatch.DrawString(font, dispScore.ToString(), new Vector2(20, 20), highScoreColor);
            }

            //draw high score
            spriteBatch.End();
            if (renderTargetEnabled)
            {
                graphics.GraphicsDevice.SetRenderTarget(null);

                //draw render target
                spriteBatch.Begin();
                spriteBatch.Draw((Texture2D)renderTarget,
                    new Vector2(0, 0),          // x,y position
                    new Rectangle(0, 0, bounds.Width, bounds.Height),   // just one grid
                    Color.White                    // no color scaling
                    );
                spriteBatch.End();
            }

            base.Draw(gameTime);

        }
    }
}
;//end game
