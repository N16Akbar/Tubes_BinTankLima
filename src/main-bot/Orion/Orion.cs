using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Orion : Bot
{

    // Menyimpan arah orbit bot, 1 untuk satu arah dan -1 untuk arah sebaliknya
    int orbitDirection = 1;

    // Menyimpan posisi bot sebagai perbandingan untuk mendeteksi apakah bot stuck
    double prevX = 0;
    double prevY = 0;
    bool hasprevPosition = false;
    int stuckTurns = 0;

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
        TracksColor = Color.Black;
        BodyColor = Color.Blue;
        RadarColor = Color.DarkSlateGray;
        GunColor = Color.LightGray;
        BulletColor = Color.DodgerBlue;
        ScanColor = Color.LightSkyBlue;

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
        // Variabel menyimpan derajat dan jarak ke bot musuh yang discan
        double enemyDirection = DirectionTo(evt.X, evt.Y);
        double enemyDistance = DistanceTo(evt.X, evt.Y);

        // Variabel menyatakan jika bot mengalami stuck
        bool isStuck = false;

        // Menyimpan posisi awal bot untuk menjadi pembanding pada scan berikutnya
        if (!hasprevPosition)
        {
            prevX = X;
            prevY = Y;
            hasprevPosition = true;
        }
        else
        {
            // Menghitung seberapa jauh bot berpindah dari posisi sebelumnya
            double moveX = X - prevX;
            double moveY = Y - prevY;

            // Jika bot hampir tidak berubah posisi padahal masih punya sisa gerakan, bot dianggap mulai stuck
            if (Math.Abs(moveX) < 1 && Math.Abs(moveY) < 1 && Math.Abs(DistanceRemaining) > MaxSpeed)
                stuckTurns++;
            else
                stuckTurns = 0;

            prevX = X;
            prevY = Y;

            // Jika stuck terjadi beberapa kali berturut-turut, bot membalik arah orbit dan bergerak ke tengah arena
            if (stuckTurns >= 3)
            {
                orbitDirection *= -1;
                isStuck = true;
                stuckTurns = 0;
            }
        }

        // Melakukan penguncian radar ke bot musuh.
        SetTurnRadarLeft(RadarBearingTo(evt.X, evt.Y));

        // Menentukan besar peluru berdasarkan jarak ke bot musuh dan energi bot tersebut
        double firePower;

        if (enemyDistance < 120)
            firePower = 3.0;
        else if (enemyDistance < 300)
            firePower = 2.0;
        else if (enemyDistance < 550)
            firePower = 1.1;
        else
            firePower = 0.7;

        // Jika energi yang dimiliki rendah, kurangi besar peluru untuk menjaga agar bot tidak mati konyol
        if (Energy < 30 && firePower > 1.0)
            firePower = 1.0;

        if (Energy < 15 && firePower > 0.7)
            firePower = 0.7;

        // Jika energi musuh hampir mendekati 0, tidak perlu memakai peluru besar
        if (evt.Energy < 15 && firePower > 1.0)
            firePower = 1.0;

        // Jika musuh dekat dan energi bot unggul, gunakan peluru besar
        if (enemyDistance < 120 && Energy > evt.Energy + 20)
            firePower = 3.0;

        // Memastikan batasan peluru di dalam range aman API
        if (firePower < 0.1)
            firePower = 0.1;
        else if (firePower > 3.0)
            firePower = 3.0;

        // Menentukan waktu prediksi berdasarkan jarak musuh
        double predictionTime;

        if (enemyDistance < 120)
            predictionTime = 0.5;
        else if (enemyDistance < 300)
            predictionTime = 1.0;
        else if (enemyDistance < 550)
            predictionTime = 1.5;
        else
            predictionTime = 2.0;

        // Mengubah arah gerak musuh dari derajat ke radian
        double enemyMoveDirection = evt.Direction * Math.PI / 180.0;

        // Menghitung perkiraan posisi musuh berikutnya
        double predictedX = evt.X + Math.Sin(enemyMoveDirection) * evt.Speed * predictionTime;
        double predictedY = evt.Y + Math.Cos(enemyMoveDirection) * evt.Speed * predictionTime;

        // Membatasi posisi prediksi X agar tetap di dalam arena
        if (predictedX < 0)
            predictedX = 0;
        else if (predictedX > ArenaWidth)
            predictedX = ArenaWidth;

        // Membatasi posisi prediksi Y agar tetap di dalam arena
        if (predictedY < 0)
            predictedY = 0;
        else if (predictedY > ArenaHeight)
            predictedY = ArenaHeight;

        // Mengarahkan gun ke posisi prediksi musuh
        double gunTurn = GunBearingTo(predictedX, predictedY);
        SetTurnGunLeft(gunTurn);

        // Jika bot stuck, bot bergerak ke tengah arena
        if (isStuck)
        {
            SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
            SetForward(180);
        }
        // Jika bot berada di area 10% terluar arena, bot bergerak ke tengah arena
        else if (X < ArenaWidth / 10 || X > ArenaWidth - ArenaWidth / 10 || Y < ArenaHeight / 10 || Y > ArenaHeight - ArenaHeight / 10)
        {
            orbitDirection *= -1;

            SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
            SetForward(180);
        }
        // Jika energi bot unggul dan musuh lemah, bot menekan musuh secara langsung
        else if (Energy > evt.Energy + 25 && Energy > 40 && evt.Energy < 30 && enemyDistance < 180)
        {
            SetTurnLeft(BearingTo(evt.X, evt.Y));
            SetForward(enemyDistance);
        }
        // Jika bot musuh jauh, mendekat ke musuh sambil melakukan sedikit orbit
        else if (enemyDistance > 250)
        {
            SetTurnLeft(CalcDeltaAngle(enemyDirection + orbitDirection * 45, Direction));
            SetForward(180);
        }
        // Jika jarak ke bot musuh cukup sedang, lakukan mengelilingi musuh
        else if (enemyDistance > 120)
        {
            SetTurnLeft(CalcDeltaAngle(enemyDirection + orbitDirection * 90, Direction));
            SetForward(120);
        }
        // Jika terlalu dekat, mundur untuk menjaga jarak
        else
        {
            SetTurnLeft(CalcDeltaAngle(enemyDirection + 180, Direction));
            SetForward(120);
        }

        // Toleransi akurasi tembakan, semakin jauh musuh, semakin kecil toleransinya.
        double aimTolerance;

        if (enemyDistance <= 150)
            aimTolerance = 6;
        else if (enemyDistance <= 350)
            aimTolerance = 4;
        else
            aimTolerance = 2;

        // Tembak jika gun sudah cukup mengarah dan tidak panas.
        if (Math.Abs(gunTurn) < aimTolerance && GunHeat == 0)
        {
            SetFire(firePower);
        }

        // Eksekusi semua perintah Set*().
        Go();
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        // Menyimpan arah dan jarak ke bot musuh yang tertabrak
        double hitDirection = DirectionTo(evt.X, evt.Y);
        double hitDistance = DistanceTo(evt.X, evt.Y);

        // Jika energi bot unggul dan jaraknya dekat, lakukan ramming sambil menembak
        if (Energy > evt.Energy + 20 && hitDistance < 120)
        {
            SetTurnLeft(BearingTo(evt.X, evt.Y));
            SetForward(hitDistance);

            double gunTurn = GunBearingTo(evt.X, evt.Y);
            SetTurnGunLeft(gunTurn);

            if (Math.Abs(gunTurn) < 10 && GunHeat == 0)
                SetFire(1.5);
        }
        else
        {
            // Jika tidak, bot akan menjauh
            orbitDirection *= -1;

            SetTurnLeft(CalcDeltaAngle(hitDirection + 180, Direction));
            SetForward(150);
        }

        // Eksekusi semua perintah Set*().
        Go();
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        // Saat bot menabrak dinding, bot bergerak kembali ke tengah arena
        orbitDirection *= -1;

        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));
        SetForward(180);

        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Saat bot terkena peluru, bot membalik arah orbit
        orbitDirection *= -1;

        // Bot bergerak menyamping setelah terkena peluru
        SetTurnLeft(90 * orbitDirection);
        SetForward(150);

        Go();
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt)
    {
        // Jika bot melewatkan giliran, bot bergerak agar tidak diam terlalu lama
        orbitDirection *= -1;

        SetTurnLeft(90 * orbitDirection);
        SetForward(120);

        Go();
    }

    public override void OnRoundStarted(RoundStartedEvent evt)
    {
        // Reset seluruh variabel agar ronde baru tidak memakai data sebelumnya
        orbitDirection = 1;

        prevX = 0;
        prevY = 0;
        hasprevPosition = false;
        stuckTurns = 0;
    }

    public override void OnConnected(ConnectedEvent evt)
    {
        // Penanda bot berhasil masuk
        Console.WriteLine("Bot Orion berhasil masuk.");
    }
}