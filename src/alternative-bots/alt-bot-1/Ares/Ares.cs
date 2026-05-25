using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Ares : Bot
{

    static void Main(string[] args)
    {
        new Ares().Start();
    }

    Ares() : base(BotInfo.FromFile("Ares.json")) { }

    public override void Run()
    {
        // Membuat pergerakan radar terpisah dari pergerakan gun
        AdjustRadarForGunTurn = true;

        // Membuat pergerakan gun terpisah dari pergerakan body
        AdjustGunForBodyTurn = true;

        // Membuat pergerakan radar terpisah dari pergerakan body
        AdjustRadarForBodyTurn = true;

        // Warna setiap bagian dari tank
        BodyColor = Color.DarkRed;
        GunColor = Color.Black;
        RadarColor = Color.Red;
        BulletColor = Color.Orange;
        ScanColor = Color.Yellow;
        TracksColor = Color.DarkRed;

        // Selama tank berjalan,
        while (IsRunning)
        {
            // lakukan pemutaran radar untuk mencari musuh
            TurnRadarLeft(45);
        }
    }

    // Override bot tank saat menemukan musuh
    public override void OnScannedBot(ScannedBotEvent evt)
    {
        // Menyimpan jarak ke bot musuh yang terdeteksi
        double enemyDistance = DistanceTo(evt.X, evt.Y);

        // Mengunci radar ke arah bot musuh agar posisi musuh terus terpantau
        SetTurnRadarLeft(RadarBearingTo(evt.X, evt.Y));

        // Menghitung besar sudut yang dibutuhkan gun untuk mengarah ke musuh
        double gunTurn = GunBearingTo(evt.X, evt.Y);

        // Mengarahkan gun ke posisi bot musuh
        SetTurnGunLeft(gunTurn);

        // Mengarahkan body langsung ke bot musuh untuk melakukan strategi ramming
        SetTurnLeft(BearingTo(evt.X, evt.Y));

        // Selalu maju ke arah bot musuh agar dapat menabrak dan memberi tekanan langsung
        SetForward(enemyDistance);

        // Menentukan besar peluru berdasarkan jarak ke bot musuh
        double firePower;

        // Jika musuh sangat dekat, gunakan peluru besar karena peluang mengenai target lebih tinggi
        if (enemyDistance < 120)
            firePower = 3.0;

        // Jika musuh berada pada jarak menengah, gunakan peluru sedang
        else if (enemyDistance < 300)
            firePower = 2.0;

        // Jika musuh cukup jauh, gunakan peluru kecil agar tidak boros energi
        else
            firePower = 1.0;

        // Jika energi bot rendah, kurangi besar peluru agar energi tetap aman
        if (Energy < 30 && firePower > 1.0)
            firePower = 1.0;

        // Jika energi bot sangat rendah, gunakan peluru kecil saja
        if (Energy < 15 && firePower > 0.7)
            firePower = 0.7;

        // Tembak jika gun sudah cukup mengarah ke musuh dan gun tidak panas
        if (Math.Abs(gunTurn) < 10 && GunHeat == 0)
        {
            SetFire(firePower);
        }

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitBot(HitBotEvent evt)
    {
        // Saat menabrak bot musuh, arahkan kembali body ke bot tersebut
        SetTurnLeft(BearingTo(evt.X, evt.Y));

        // Tetap maju untuk mempertahankan tekanan ramming terhadap musuh
        SetForward(DistanceTo(evt.X, evt.Y));

        // Menghitung besar sudut yang dibutuhkan gun untuk mengarah ke bot yang tertabrak
        double gunTurn = GunBearingTo(evt.X, evt.Y);

        // Mengarahkan gun ke bot yang tertabrak
        SetTurnGunLeft(gunTurn);

        // Tembak saat posisi musuh sangat dekat dan gun sudah cukup mengarah
        if (Math.Abs(gunTurn) < 10 && GunHeat == 0)
        {
            SetFire(2.0);
        }

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitWall(HitWallEvent evt)
    {
        // Jika menabrak dinding, bot diarahkan kembali ke tengah arena
        // agar tidak terus tersangkut di tepi arena
        SetTurnLeft(CalcDeltaAngle(DirectionTo(ArenaWidth / 2, ArenaHeight / 2), Direction));

        // Maju menuju tengah arena untuk mendapatkan ruang gerak yang lebih aman
        SetForward(180);

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnHitByBullet(HitByBulletEvent evt)
    {
        // Saat terkena peluru, bot tetap bergerak agar tidak diam dan mudah ditembak lagi
        SetForward(120);

        // Eksekusi semua perintah Set*()
        Go();
    }

    public override void OnSkippedTurn(SkippedTurnEvent evt)
    {
        // Jika turn terlewat, bot tetap maju agar strategi agresif terus berjalan
        SetForward(120);

        // Eksekusi semua perintah Set*()
        Go();
    }
}
