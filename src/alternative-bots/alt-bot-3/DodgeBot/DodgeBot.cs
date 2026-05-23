using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class DodgeBot : Bot
{
    Random random = new Random();

    static void Main(string[] args)
    {
        new DodgeBot().Start();
    }

    DodgeBot() : base(BotInfo.FromFile("DodgeBot.json")) { }

    public override void Run()
    {
        BodyColor = Color.Blue;
        TurretColor = Color.Black;
        RadarColor = Color.Yellow;
        BulletColor = Color.Red;

        // Radar berputar terus
        while (IsRunning)
        {
            SetTurnRadarRight(360);

            // Gerakan acak agar sulit ditembak
            Forward(100);

            SetTurnRight(random.Next(30, 120));

            SetForward(100);

            SetTurnLeft(random.Next(30, 120));
        }
    }

    // Saat melihat musuh
    public override void OnScannedBot(ScannedBotEvent e)
    {
        Console.WriteLine("Enemy detected!");

        // Arahkan turret ke musuh
        SetTurnGunRight(DirectionTo(e.X, e.Y) - GunDirection);

        // Tembak jika musuh terlihat
        SetFire(8);

        // Gerakan menghindar setelah menembak
        SetTurnRight(90);
        SetForward(150);
    }

    // Saat terkena peluru
    public override void OnHitByBullet(HitByBulletEvent e)
    {
        Console.WriteLine("I'm hit! Dodging!");

        // Gerakan zig-zag menghindar
        SetTurnRight(90);
        SetForward(200);

        SetTurnLeft(45);
        SetBack(100);
    }

    // Saat menabrak dinding
    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Wall hit!");

        SetBack(100);
        SetTurnRight(90);
    }

    // Saat menabrak bot lain
    public override void OnHitBot(HitBotEvent e)
    {
        Console.WriteLine("Bot collision!");

        SetBack(50);
        SetTurnRight(60);

        SetFire(8);
    }
}