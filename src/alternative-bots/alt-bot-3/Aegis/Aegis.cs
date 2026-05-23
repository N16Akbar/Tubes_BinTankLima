using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Aegis : Bot
{
    static void Main(string[] args)
    {
        new Aegis().Start();
    }

    Aegis() : base(BotInfo.FromFile("Aegis.json")) { }

    public override void Run()
    {
        BodyColor = Color.Blue;
        TurretColor = Color.Black;
        RadarColor = Color.Yellow;
        BulletColor = Color.Red;

        AdjustRadarForGunTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForBodyTurn = true;

        while (IsRunning)
        {
            SetTurnRadarRight(45);

            SetTurnLeft(45);
            SetForward(120);

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent evt)
    {
        double enemyDistance = DistanceTo(evt.X, evt.Y);

        SetTurnRadarLeft(RadarBearingTo(evt.X, evt.Y));

        double gunTurn = GunBearingTo(evt.X, evt.Y);
        SetTurnGunLeft(gunTurn);

        if (X < 100 || X > ArenaWidth - 100 || Y < 100 || Y > ArenaHeight - 100)
        {
            SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
            SetForward(180);
        }
        else if (enemyDistance < 180)
        {
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 140, Direction));
            SetForward(180);
        }
        else if (enemyDistance < 350)
        {
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 100, Direction));
            SetForward(150);
        }
        else
        {
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 90, Direction));
            SetForward(120);
        }

        if (enemyDistance < 200 && Energy > 45 && Math.Abs(gunTurn) < 6 && GunHeat == 0)
        {
            SetFire(0.8);
        }

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        SetTurnLeft(100);
        SetForward(180);

        Go();
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
        SetForward(220);

        Go();
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
        SetForward(160);

        Go();
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt)
    {
        SetTurnLeft(90);
        SetForward(120);

        Go();
    }
}