using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Orion : Bot
{
    // Variabel untuk state musuh ditemukan atau tidak
    bool enemyFound;

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
        // Menandakan bot masih belum menemukan musuh
        enemyFound = false;

        // Warna Bot
        BodyColor = Color.Gold;
        TurretColor = Color.Black;
        RadarColor = Color.White;
        BulletColor = Color.Yellow;
        GunColor = Color.Goldenrod;

        // Selama bot berjalan
        while (IsRunning)
        {
            // Jika bot belum ketemu, terus scan dengan memutar gun ke kanan selama 5 derajat per tick
            if (!enemyFound)
                TurnGunRight(5);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Menandakan musuh telah ditemukan
        enemyFound = true;

        // double direction = DirectionTo(evt.X, evt.Y);

        // Kalkulasi berapa derajat diberikan untuk memutar gun ke arah musuh yang discan
        double gunTurn = GunBearingTo(evt.X, evt.Y);

        // Kalkulasi berapa jauh jarak bot ini dengan bot musuh yang discan
        double distance = DistanceTo(evt.X, evt.Y);

        /*
            double directionRightNow = Direction;
            double directionCalculated = BearingTo(evt.X, evt.Y);
            double directionCalculated = direction - directionRightNow;
            while (directionCalculated > 180) directionCalculated -= 360;
            while (directionCalculated < -180) directionCalculated += 360;
            SetTurnRight(directionCalculated);
        */

        // Secara bersamaan putar gun ke arah musuh dan maju hingga tersisa jarak sebesar 100 dari musuh
        SetTurnGunRight(gunTurn);
        SetForward(distance - 100);

        // Kalkulasi besar peluru berdasarkan jarak ke arah musuh
        double fireSize = 300 / distance;
        // Jika besarnya lebih dari 3, batasi hingga batas max peluru yg ada di game ini, yaitu 3.0
        if (fireSize > 3.0)
        {
            fireSize = 3.0;
        }
        // Jika besarnya kurang dari 1, batasi hingga batas min peluru yang ada, yaitu 1.0
        else if (fireSize < 1.0)
        {
            fireSize = 1.0;
        }

        // Jika gun tidak panas, maka tembak
        if (GunHeat == 0)
        {
            SetFire(fireSize);
        }

        // Mengeksekusi seluruh fungsi yang terdefinisi lalu rescan ulang dan menyatakan musuh tidak ditemukan
        Go();
        Rescan();
        enemyFound = false;
    }

    /* Akan dibuat di versi selanjutnya
        public override void OnBulletHit(BulletHitBotEvent bulletHitBotEvent)
        {
        }

        public override void OnHitByBullet(HitByBulletEvent evt)
        {
            var bearing = CalcBearing(evt.Bullet.Direction);

            Back(100);
            TurnLeft(90 - bearing);
        }

        public override void OnHitBot(HitBotEvent botHitBotEvent)
        {
            double direction = DirectionTo(botHitBotEvent.X, botHitBotEvent.Y);
            Forward(100);
        }
    */
}