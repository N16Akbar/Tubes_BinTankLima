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
        // Warna setiap bagian dari tank
        BodyColor = Color.Blue;
        TurretColor = Color.Black;
        RadarColor = Color.Yellow;
        BulletColor = Color.Red;

        // Membuat pergerakan radar terpisah dari pergerakan gun
        AdjustRadarForGunTurn = true;

        // Membuat pergerakan gun terpisah dari pergerakan body
        AdjustGunForBodyTurn = true;

        // Membuat pergerakan radar terpisah dari pergerakan body
        AdjustRadarForBodyTurn = true;

        // Selama tank berjalan,
        while (IsRunning)
        {
            // Radar terus diputar untuk mencari bot musuh
            SetTurnRadarRight(45);

            // Bot tetap bergerak dan berbelok agar tidak diam di satu posisi
            SetTurnLeft(45);
            SetForward(120);

            // Eksekusi semua perintah Set*()
            Go();
        }
    }

    // Override bot tank saat menemukan musuh
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Menyimpan jarak ke bot musuh yang terdeteksi
        double enemyDistance = DistanceTo(evt.X, evt.Y);

        // Mengunci radar ke arah bot musuh agar posisi musuh tetap terpantau
        SetTurnRadarLeft(RadarBearingTo(evt.X, evt.Y));

        // Menghitung besar sudut yang dibutuhkan gun untuk mengarah ke musuh
        double gunTurn = GunBearingTo(evt.X, evt.Y);

        // Mengarahkan gun ke posisi bot musuh
        SetTurnGunLeft(gunTurn);

        // Jika bot berada dekat dinding, prioritaskan bergerak ke tengah arena
        if (X < 100 || X > ArenaWidth - 100 || Y < 100 || Y > ArenaHeight - 100)
        {
            // Mengarahkan body ke tengah arena agar bot keluar dari area berbahaya
            SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));

            // Maju menuju tengah arena untuk mendapatkan ruang gerak yang lebih aman
            SetForward(180);
        }

        // Jika musuh terlalu dekat, bot menjauh dengan sudut besar
        else if (enemyDistance < 180)
        {
            // Bergerak menjauh dari arah musuh agar tidak mudah terkena serangan langsung
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 140, Direction));
            SetForward(180);
        }

        // Jika musuh berada pada jarak menengah, bot bergerak menyamping
        else if (enemyDistance < 350)
        {
            // Bergerak menyamping terhadap musuh agar lebih sulit ditembak
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 100, Direction));
            SetForward(150);
        }

        // Jika musuh cukup jauh, bot tetap bergerak menyamping untuk menjaga mobilitas
        else
        {
            // Mengambil sudut 90 derajat dari arah musuh untuk mempertahankan gerakan defensif
            SetTurnLeft(CalcDeltaAngle(DirectionTo(evt.X, evt.Y) + 90, Direction));
            SetForward(120);
        }

        // Aegis hanya menembak saat kondisi cukup aman:
        // musuh cukup dekat, energi bot masih tinggi, gun sudah akurat, dan gun tidak panas
        if (enemyDistance < 200 && Energy > 45 && Math.Abs(gunTurn) < 6 && GunHeat == 0)
        {
            SetFire(0.8);
        }

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Saat terkena peluru, bot langsung mengubah arah gerak
        // agar tidak mudah ditembak lagi dari arah yang sama
        SetTurnLeft(100);

        // Maju untuk berpindah dari posisi sebelumnya
        SetForward(180);

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        // Jika menabrak dinding, bot diarahkan kembali ke tengah arena
        // agar tidak tersangkut di tepi arena
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));

        // Maju lebih jauh menuju tengah arena
        SetForward(220);

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        // Jika menabrak bot lain, Aegis tidak melanjutkan ramming
        // karena strategi utama Aegis adalah bertahan dan menjaga posisi aman
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));

        // Bergerak ke tengah arena untuk menghindari kontak lanjutan
        SetForward(160);

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt)
    {
        // Jika turn terlewat, bot tetap bergerak agar tidak diam terlalu lama
        SetTurnLeft(90);
        SetForward(120);

        // Eksekusi semua perintah Set*()
        Go();
    }
}