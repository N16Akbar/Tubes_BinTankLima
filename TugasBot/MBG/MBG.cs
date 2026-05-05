using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class MBG : Bot
{
    // Variabel untuk state musuh ditemukan atau tidak
    bool enemyFound;
    static void Main(string[] args)
    {
        new MBG().Start();
    }
    MBG() : base(BotInfo.FromFile("MBG.json")) { }

    public override void Run()
    {
        enemyFound = false;

        BodyColor = Color.Gold;
        TurretColor = Color.Black;
        RadarColor = Color.White;
        BulletColor = Color.Yellow;
        GunColor = Color.Goldenrod;

        while (IsRunning)
        {
            if (!enemyFound)
                TurnGunRight(5);
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        enemyFound = true;
        double direction = DirectionTo(evt.X, evt.Y);
        double distance = DistanceTo(evt.X, evt.Y);
        double directionRightNow = Direction;
        while (enemyFound)
        {
            double directionCalculated = direction - directionRightNow;
            while (directionCalculated > 180) directionCalculated -= 360;
            while (directionCalculated < -180) directionCalculated += 360;

            if (distance >= 300)
            {
                Fire(1.0);
            }
            else if (distance >= 100 && distance < 300)
            {
                TurnGunLeft(10);
                Fire(2.0);
            }
            else
            {
                TurnGunLeft(10);
                Fire(3.0);
            }
            TurnRight(directionCalculated);
            Forward(distance - 100);
            enemyFound = false;
        }
    }

    // public override void OnBulletHit(BulletHitBotEvent bulletHitBotEvent)
    // {
    // }

    // public override void OnHitByBullet(HitByBulletEvent evt)
    // {
    //     var bearing = CalcBearing(evt.Bullet.Direction);

    //     Back(100);
    //     TurnLeft(90 - bearing);
    // }

    // public override void OnHitBot(HitBotEvent botHitBotEvent)
    // {
    //     double direction = DirectionTo(botHitBotEvent.X, botHitBotEvent.Y);
    //     Forward(100);
    // }
}

