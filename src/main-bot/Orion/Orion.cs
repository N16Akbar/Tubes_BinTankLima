using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Orion : Bot
{
    double enemyDirection;
    double enemyDistance;

    int orbitDirection = 1;
    int moveCounter = 0;

    int escapeTicks = 0;
    int escapeCooldown = 0;

    double lastX = 0;
    double lastY = 0;
    bool hasLastPosition = false;
    int stuckCounter = 0;

    static void Main(string[] args)
    {
        new Orion().Start();
    }

    Orion() : base(BotInfo.FromFile("Orion.json")) { }

    public override void Run()
    {
        // Membuat pergerakan radar terpisah dari pergerakan gun
        AdjustRadarForGunTurn = true;

        // Membuat pergerakan gun terpisah dari pergerakan body
        AdjustGunForBodyTurn = true;

        // Membuat pergerakan radar terpisah dari pergerakan body
        AdjustRadarForBodyTurn = true;

        // Warna setiap bagian dari tank
        RadarColor = Color.FromArgb(35, 35, 40);        
        GunColor = Color.FromArgb(180, 185, 190);       
        BulletColor = Color.FromArgb(80, 140, 255);     
        ScanColor = Color.FromArgb(120, 190, 255);      
        TracksColor = Color.FromArgb(25, 25, 30);       
        BodyColor = Color.FromArgb(38, 75, 150);        

        // Selama tank berjalan,
        while (IsRunning)
        {
            // lakukan pemutaran radar sebesar 45 derajat dalam satu tick (ms)
            TurnRadarLeft(45);
        }
    }

    // Override bot tank saat menemukan musuh (disimpan dalam variabel evt)
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Variabel menyimpan derajat yang dibutuhkan dari derajat bot saat ini ke derajat musuh
        enemyDirection = DirectionTo(evt.X, evt.Y);

        // Variabel menyimpan jarak yang dibutuhkan dari jarak bot saat ini ke jarak musuh
        enemyDistance = DistanceTo(evt.X, evt.Y);

        // Mengecek jika bot mengalami stuck
        CheckStuck();

        // Melakukan subprogram lock radar ke musuh
        LockRadar();

        // Variabel yang menyimpan seberapa besar peluru yang ditembak berdasarkan hasil subprogram ChooseFirePower()
        double firePower = ChooseFirePower(evt);

        // Variabel yang menyimpan seberapa besar derajat yang dibutuhkan untuk mengarahkan gun saat ini ke arah musuh
        double gunTurn = AimGun(evt, firePower);

        // Melakukan pergerakan zig-zag dan orbit sesuai kondisi yang ada pada subprogram
        MoveGreedy(evt);

        // Jika arah gun sudah di dalam batas toleransi,
        if (CanShoot(gunTurn))
        {
            // lakukan penembakan dengan besar peluru firePower
            SetFire(firePower);
        }

        // Eksekusi semua Set*() yang didefinisikan
        Go();
    }

    // Subprogram melakukan penguncian radar ke arah musuh berdasarkan perhitungan delta derajat saat ini ke derajat musuh
    public void LockRadar() => SetTurnRadarLeft(CalcDeltaAngle(enemyDirection, RadarDirection));

    public double ChooseFirePower(ScannedBotEvent evt)
    {
        double power;

        if (enemyDistance < 120)
            power = 3.0;
        else if (enemyDistance < 300)
            power = 2.0;
        else if (enemyDistance < 550)
            power = 1.1;
        else
            power = 0.7;

        if (Energy < 30)
            power = Math.Min(power, 1.0);

        if (Energy < 15)
            power = Math.Min(power, 0.7);

        if (evt.Energy < 15)
            power = Math.Min(power, 1.0);

        if (enemyDistance < 100 && Energy > evt.Energy + 20 && Energy > 40)
            power = 3.0;

        return Clamp(power, 0.1, 3.0);
    }

    private double AimGun(ScannedBotEvent evt, double firePower)
    {
        double bulletSpeed = CalcBulletSpeed(firePower);
        double enemyHeadingRad = evt.Direction * Math.PI / 180.0;
        double enemyVelocityX = Math.Sin(enemyHeadingRad) * evt.Speed;
        double enemyVelocityY = Math.Cos(enemyHeadingRad) * evt.Speed;
        double predictedX = evt.X;
        double predictedY = evt.Y;
        double angleToEnemyRad = enemyDirection * Math.PI / 180.0;

        double lateralVelocity =
            Math.Abs(
                enemyVelocityX * Math.Cos(angleToEnemyRad) -
                enemyVelocityY * Math.Sin(angleToEnemyRad)
            );

        double leadFactor = Clamp(lateralVelocity / MaxSpeed, 0.20, 1.00);
        if (enemyDistance > 500)
            leadFactor *= 0.80;
        if (enemyDistance < 120)
            leadFactor *= 0.60;
        for (int i = 0; i < 5; i++)
        {
            double predictedDistance = DistanceTo(predictedX, predictedY);
            double travelTime = predictedDistance / bulletSpeed;

            predictedX = evt.X + enemyVelocityX * travelTime * leadFactor;
            predictedY = evt.Y + enemyVelocityY * travelTime * leadFactor;
            predictedX = Clamp(predictedX, 18, ArenaWidth - 18);
            predictedY = Clamp(predictedY, 18, ArenaHeight - 18);
        }

        double aimDirection = DirectionTo(predictedX, predictedY);
        double gunTurn = CalcDeltaAngle(aimDirection, GunDirection);

        SetTurnGunLeft(gunTurn);

        return gunTurn;
    }

    private void MoveGreedy(ScannedBotEvent evt)
    {
        moveCounter++;

        if (escapeCooldown > 0)
            escapeCooldown--;
        if (escapeTicks > 0)
        {
            escapeTicks--;
            MoveToCenter();
            return;
        }
        if (moveCounter % 35 == 0)
            orbitDirection *= -1;

        if (IsNearWall())
        {
            StartEscape(15, 20);
            MoveToCenter();
            return;
        }

        bool shouldPressure =
            Energy > evt.Energy + 25 &&
            Energy > 40 &&
            evt.Energy < 30;

        if (shouldPressure && enemyDistance < 180)
        {
            SetTurnLeft(CalcDeltaAngle(enemyDirection, Direction));
            SetForward(enemyDistance + 30);
            return;
        }

        if (enemyDistance > 250)
        {
            double approachAngle = enemyDirection + orbitDirection * 25;
            SetTurnLeft(CalcDeltaAngle(approachAngle, Direction));
            SetForward(180);
        }
        else if (enemyDistance > 120)
        {
            double orbitAngle = enemyDirection + orbitDirection * 75;
            SetTurnLeft(CalcDeltaAngle(orbitAngle, Direction));
            SetForward(120);
        }
        else
        {
            double retreatAngle = enemyDirection + orbitDirection * 100;
            SetTurnLeft(CalcDeltaAngle(retreatAngle, Direction));
            SetBack(100);
        }
    }

    private bool CanShoot(double gunTurn)
    {
        double tolerance;

        if (enemyDistance <= 150)
            tolerance = 6;
        else if (enemyDistance <= 350)
            tolerance = 4;
        else
            tolerance = 2;

        return Math.Abs(gunTurn) < tolerance && GunHeat == 0;
    }

    private void CheckStuck()
    {
        if (!hasLastPosition)
        {
            lastX = X;
            lastY = Y;
            hasLastPosition = true;
            return;
        }

        double dx = X - lastX;
        double dy = Y - lastY;
        double movedDistance = Math.Sqrt(dx * dx + dy * dy);
        bool barelyMoved = movedDistance < 2;
        bool stillTryingToMove = Math.Abs(DistanceRemaining) > 20;

        if (barelyMoved && stillTryingToMove)
            stuckCounter++;
        else
            stuckCounter = 0;

        lastX = X;
        lastY = Y;

        if (stuckCounter >= 5)
        {
            StartEscape(18, 15);
            stuckCounter = 0;
        }
    }

    private void StartEscape(int ticks, int cooldown)
    {
        escapeTicks = ticks;

        if (escapeCooldown == 0)
        {
            orbitDirection *= -1;
            escapeCooldown = cooldown;
        }
    }

    private void MoveToCenter()
    {
        double centerX = ArenaWidth / 2;
        double centerY = ArenaHeight / 2;

        double centerDirection = DirectionTo(centerX, centerY);
        double escapeAngle = centerDirection + orbitDirection * 25;

        SetTurnLeft(CalcDeltaAngle(escapeAngle, Direction));
        SetForward(180);
    }

    private bool IsNearWall()
    {
        double margin = 85;

        return X < margin ||
               X > ArenaWidth - margin ||
               Y < margin ||
               Y > ArenaHeight - margin;
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        double hitDirection = DirectionTo(evt.X, evt.Y);
        double hitDistance = DistanceTo(evt.X, evt.Y);

        bool shouldRam =
            Energy > evt.Energy + 20 &&
            Energy > 40 &&
            evt.Energy < 30 &&
            hitDistance < 120;

        if (shouldRam)
        {
            SetTurnLeft(CalcDeltaAngle(hitDirection, Direction));
            SetForward(hitDistance + 50);

            double gunTurn = CalcDeltaAngle(hitDirection, GunDirection);
            SetTurnGunLeft(gunTurn);

            if (Math.Abs(gunTurn) < 10 && GunHeat == 0)
                SetFire(1.5);
        }
        else
        {
            StartEscape(12, 10);

            double awayAngle = hitDirection + 180 + orbitDirection * 35;

            SetTurnLeft(CalcDeltaAngle(awayAngle, Direction));
            SetForward(150);
        }

        Go();
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        StartEscape(18, 25);
        MoveToCenter();
        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        StartEscape(6, 8);
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt)
    {
        StartEscape(8, 10);
    }

    public override void OnRoundStarted(RoundStartedEvent evt)
    {
        orbitDirection = 1;
        moveCounter = 0;

        escapeTicks = 0;
        escapeCooldown = 0;

        lastX = 0;
        lastY = 0;
        hasLastPosition = false;
        stuckCounter = 0;
    }

    public override void OnConnected(ConnectedEvent evt)
    {
        Console.WriteLine("Bot Orion berhasil masuk.");
    }

    private double Clamp(double value, double min, double max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}