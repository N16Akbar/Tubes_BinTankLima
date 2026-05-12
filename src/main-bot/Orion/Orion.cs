using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

/*
TODO: coba buat kalkulasi agar pelurunya dapat memprediksi arah musuh agar tepat sasaran
*/

public class Orion : Bot
{
    // Memulai bot
    static void Main(string[] args)
    {
        new Orion().Start();
    }

    // Memuat data tentang bot
    Orion() : base(BotInfo.FromFile("Orion.json")) { }

    // Override fungsi Run() ketika bot berjalan
    public override void Run()
    {
        // Memisahkan pergerakan agar tidak saling memengaruhi saat bodi berputar
        AdjustRadarForGunTurn = true;
        AdjustGunForBodyTurn = true;

        // Warna Bot
        BodyColor = Color.Gold;
        TurretColor = Color.Black;
        RadarColor = Color.White;
        BulletColor = Color.Yellow;
        GunColor = Color.Goldenrod;

        // Selama bot berjalan
        while (IsRunning)
        {
            // Putar radar secara menyeluruh dalam 1 tick game.
            TurnRadarLeft(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Kalkulasi berapa jarak antara bot ke arah musuh yang discan
        double directionToEnemy = DirectionTo(evt.X, evt.Y);

        // Kalkulasi berapa derajat diberikan untuk memutar radar ke arah musuh yang discan
        double radarTurn = CalcDeltaAngle(directionToEnemy, RadarDirection);
        SetTurnRadarLeft(radarTurn);

        // Kalkulasi berapa derajat diberikan untuk memutar gun ke arah musuh yang discan
        double gunTurn = CalcDeltaAngle(directionToEnemy, GunDirection);
        SetTurnGunLeft(gunTurn);

        // Kalkulasi berapa derajat diberikan untuk memutar body ke arah musuh yang discan
        double bodyTurn = CalcDeltaAngle(directionToEnemy, Direction);
        SetTurnLeft(bodyTurn);

        // Hitung jarak ke musuh
        double distanceToEnemy = DistanceTo(evt.X, evt.Y);
        Console.WriteLine("Jarak ke bot musuh saat ini: " + distanceToEnemy);

        // Terus bergerak maju mendekati musuh
        if (distanceToEnemy > 100)
        {
            SetForward(distanceToEnemy - 100);
        }

        // Menembak jika dalam rentang toleransi direction < 10
        if (Math.Abs(gunTurn) < 10)
        {
            // Tembakan dinamis: makin dekat musuh, makin besar daya (maks 3.0)
            double firePower = Math.Min(400 / distanceToEnemy, 3.0);
            double bulletSpeed = CalcBulletSpeed(firePower);
            Console.WriteLine("Kecepatan peluru saat ini: " + bulletSpeed);
            SetFire(firePower);
        }

        // Eksekusi semua perintah pergerakan dan tembakan di atas secara serentak
        Go();
    }

    // public override void OnBotDeath(BotDeathEvent botDeathEvent)
    // {
    //     SetFireAssist
    // }
}