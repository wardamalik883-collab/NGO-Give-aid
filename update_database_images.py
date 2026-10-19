import subprocess

statements = []

# 1. Update NGO Logo
statements.append("UPDATE NGOs SET LogoPath = '/uploads/ngos/give-aid-relief-foundation.png' WHERE Id = 1;")

# 2. Update Causes ImagePath
statements.append("UPDATE Causes SET ImagePath = '/uploads/causes/clean-drinking-water-thar.png' WHERE Id = 1;")
statements.append("UPDATE Causes SET ImagePath = '/uploads/causes/emergency-medical-relief-riverine.jpg' WHERE Id = 2;")
statements.append("UPDATE Causes SET ImagePath = '/uploads/causes/child-education-digital-literacy.jpg' WHERE Id = 3;")
statements.append("UPDATE Causes SET ImagePath = '/uploads/causes/winter-warmth-food-ration.jpg' WHERE Id = 4;")

# 3. Update Programmes BannerImagePath
statements.append("UPDATE Programmes SET BannerImagePath = '/uploads/programmes/mobile-health-clinic-diagnostic-camp.jpg' WHERE Id = 4;")
statements.append("UPDATE Programmes SET BannerImagePath = '/uploads/programmes/winter-warmth-ration-distribution-drive.jpg' WHERE Id = 5;")
statements.append("UPDATE Programmes SET BannerImagePath = '/uploads/programmes/clean-water-aquifer-verification-mission.png' WHERE Id = 6;")

# 4. Refresh GalleryImages table
statements.append("DELETE FROM GalleryImages;")

# 15 Clean Water images (ProgrammeId = 6)
for i in range(1, 16):
    num = f"{i:02d}"
    fn = f"Clean_Water_Aquifer_Verification_{num}.jpg"
    title = f"Clean Water Aquifer Verification - Camp #{num}"
    caption = "Field team conducting water filtration, aquifer depth testing, and clean supply installations."
    alt = f"Clean Water Aquifer Verification Mission field photo {num}"
    fp = f"/uploads/gallery/{fn}"
    statements.append(f"INSERT INTO GalleryImages (Title, Caption, AltText, FilePath, UploadedAt, ProgrammeId) VALUES ('{title}', '{caption}', '{alt}', '{fp}', GETUTCDATE(), 6);")

# 15 Mobile Health images (ProgrammeId = 4)
for i in range(1, 16):
    num = f"{i:02d}"
    fn = f"Mobile_Health_Clinic_Diagnostic_Camp_{num}.jpg"
    title = f"Mobile Health Clinic & Diagnostic Camp - Session #{num}"
    caption = "Doctors and nurses providing free clinical screenings, primary diagnostics, and medicines."
    alt = f"Mobile Health Clinic and Diagnostic Camp field photo {num}"
    fp = f"/uploads/gallery/{fn}"
    statements.append(f"INSERT INTO GalleryImages (Title, Caption, AltText, FilePath, UploadedAt, ProgrammeId) VALUES ('{title}', '{caption}', '{alt}', '{fp}', GETUTCDATE(), 4);")

# 15 Winter Warmth images (ProgrammeId = 5)
for i in range(1, 16):
    num = f"{i:02d}"
    fn = f"Winter_Warmth_Ration_Distribution_{num}.jpg"
    title = f"Winter Warmth & Ration Distribution - Relief Drive #{num}"
    caption = "Volunteers distributing thermal blankets, winter clothing, and monthly staple food packs."
    alt = f"Winter Warmth and Ration Distribution Drive relief distribution photo {num}"
    fp = f"/uploads/gallery/{fn}"
    statements.append(f"INSERT INTO GalleryImages (Title, Caption, AltText, FilePath, UploadedAt, ProgrammeId) VALUES ('{title}', '{caption}', '{alt}', '{fp}', GETUTCDATE(), 5);")

sql_script = "\n".join(statements)
with open(r"d:\Give-AID NGO\update_images.sql", "w", encoding="utf-8") as f:
    f.write(sql_script)

print(f"Generated update_images.sql with {len(statements)} statements.")

cmd = ["sqlcmd", "-S", r".\SQLEXPRESS", "-d", "GiveAidDb", "-C", "-i", r"d:\Give-AID NGO\update_images.sql"]
res = subprocess.run(cmd, capture_output=True, text=True)
print("Return Code:", res.returncode)
print("Output:", res.stdout[:500])
if res.stderr:
    print("Stderr:", res.stderr)
