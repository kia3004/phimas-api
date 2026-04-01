START TRANSACTION;

DROP TEMPORARY TABLE IF EXISTS seed_numbers;
CREATE TEMPORARY TABLE seed_numbers (
    n INT NOT NULL PRIMARY KEY
);

INSERT INTO seed_numbers (n)
VALUES
    (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),
    (11),(12),(13),(14),(15),(16),(17),(18),(19),(20);

DROP TEMPORARY TABLE IF EXISTS seed_weeks;
CREATE TEMPORARY TABLE seed_weeks (
    WeekNo INT NOT NULL PRIMARY KEY,
    WeekStart DATE NOT NULL
);

INSERT INTO seed_weeks (WeekNo, WeekStart)
VALUES
    (1, '2026-01-19'),
    (2, '2026-01-26'),
    (3, '2026-02-02'),
    (4, '2026-02-09'),
    (5, '2026-02-16'),
    (6, '2026-02-23'),
    (7, '2026-03-02'),
    (8, '2026-03-09'),
    (9, '2026-03-16'),
    (10, '2026-03-23');

DROP TEMPORARY TABLE IF EXISTS seed_profile_cases;
CREATE TEMPORARY TABLE seed_profile_cases (
    ProfileName VARCHAR(64) NOT NULL,
    WeekNo INT NOT NULL,
    CaseCount INT NOT NULL,
    PRIMARY KEY (ProfileName, WeekNo)
);

INSERT INTO seed_profile_cases (ProfileName, WeekNo, CaseCount)
VALUES
    ('dengue_hotspot', 1, 4), ('dengue_hotspot', 2, 5), ('dengue_hotspot', 3, 7), ('dengue_hotspot', 4, 10), ('dengue_hotspot', 5, 13), ('dengue_hotspot', 6, 15), ('dengue_hotspot', 7, 17), ('dengue_hotspot', 8, 16), ('dengue_hotspot', 9, 14), ('dengue_hotspot', 10, 12),
    ('dengue_rise', 1, 2), ('dengue_rise', 2, 3), ('dengue_rise', 3, 4), ('dengue_rise', 4, 6), ('dengue_rise', 5, 8), ('dengue_rise', 6, 10), ('dengue_rise', 7, 11), ('dengue_rise', 8, 10), ('dengue_rise', 9, 8), ('dengue_rise', 10, 6),
    ('dengue_medium', 1, 2), ('dengue_medium', 2, 3), ('dengue_medium', 3, 4), ('dengue_medium', 4, 5), ('dengue_medium', 5, 7), ('dengue_medium', 6, 9), ('dengue_medium', 7, 10), ('dengue_medium', 8, 9), ('dengue_medium', 9, 8), ('dengue_medium', 10, 6),
    ('dengue_tanqui', 1, 2), ('dengue_tanqui', 2, 3), ('dengue_tanqui', 3, 4), ('dengue_tanqui', 4, 6), ('dengue_tanqui', 5, 7), ('dengue_tanqui', 6, 9), ('dengue_tanqui', 7, 10), ('dengue_tanqui', 8, 9), ('dengue_tanqui', 9, 7), ('dengue_tanqui', 10, 6),
    ('dengue_pagdaraoan', 1, 2), ('dengue_pagdaraoan', 2, 3), ('dengue_pagdaraoan', 3, 5), ('dengue_pagdaraoan', 4, 6), ('dengue_pagdaraoan', 5, 8), ('dengue_pagdaraoan', 6, 10), ('dengue_pagdaraoan', 7, 12), ('dengue_pagdaraoan', 8, 11), ('dengue_pagdaraoan', 9, 9), ('dengue_pagdaraoan', 10, 7),
    ('watch_moderate', 1, 1), ('watch_moderate', 2, 2), ('watch_moderate', 3, 3), ('watch_moderate', 4, 4), ('watch_moderate', 5, 5), ('watch_moderate', 6, 6), ('watch_moderate', 7, 7), ('watch_moderate', 8, 6), ('watch_moderate', 9, 5), ('watch_moderate', 10, 4),
    ('flu_hotspot', 1, 4), ('flu_hotspot', 2, 5), ('flu_hotspot', 3, 7), ('flu_hotspot', 4, 9), ('flu_hotspot', 5, 12), ('flu_hotspot', 6, 14), ('flu_hotspot', 7, 16), ('flu_hotspot', 8, 15), ('flu_hotspot', 9, 13), ('flu_hotspot', 10, 11),
    ('flu_watch', 1, 2), ('flu_watch', 2, 3), ('flu_watch', 3, 4), ('flu_watch', 4, 5), ('flu_watch', 5, 6), ('flu_watch', 6, 7), ('flu_watch', 7, 6), ('flu_watch', 8, 5), ('flu_watch', 9, 4), ('flu_watch', 10, 3),
    ('flu_recovery', 1, 2), ('flu_recovery', 2, 3), ('flu_recovery', 3, 4), ('flu_recovery', 4, 5), ('flu_recovery', 5, 6), ('flu_recovery', 6, 7), ('flu_recovery', 7, 7), ('flu_recovery', 8, 6), ('flu_recovery', 9, 5), ('flu_recovery', 10, 4),
    ('flu_biday', 1, 2), ('flu_biday', 2, 3), ('flu_biday', 3, 4), ('flu_biday', 4, 5), ('flu_biday', 5, 7), ('flu_biday', 6, 8), ('flu_biday', 7, 9), ('flu_biday', 8, 8), ('flu_biday', 9, 6), ('flu_biday', 10, 5),
    ('flu_medium', 1, 3), ('flu_medium', 2, 4), ('flu_medium', 3, 5), ('flu_medium', 4, 6), ('flu_medium', 5, 8), ('flu_medium', 6, 10), ('flu_medium', 7, 11), ('flu_medium', 8, 10), ('flu_medium', 9, 8), ('flu_medium', 10, 6),
    ('flu_camansi', 1, 2), ('flu_camansi', 2, 3), ('flu_camansi', 3, 4), ('flu_camansi', 4, 6), ('flu_camansi', 5, 7), ('flu_camansi', 6, 9), ('flu_camansi', 7, 10), ('flu_camansi', 8, 9), ('flu_camansi', 9, 7), ('flu_camansi', 10, 5),
    ('covid_hotspot', 1, 3), ('covid_hotspot', 2, 4), ('covid_hotspot', 3, 5), ('covid_hotspot', 4, 7), ('covid_hotspot', 5, 9), ('covid_hotspot', 6, 11), ('covid_hotspot', 7, 13), ('covid_hotspot', 8, 15), ('covid_hotspot', 9, 14), ('covid_hotspot', 10, 12),
    ('covid_watch', 1, 1), ('covid_watch', 2, 2), ('covid_watch', 3, 3), ('covid_watch', 4, 4), ('covid_watch', 5, 5), ('covid_watch', 6, 6), ('covid_watch', 7, 7), ('covid_watch', 8, 7), ('covid_watch', 9, 6), ('covid_watch', 10, 5),
    ('covid_recovery', 1, 1), ('covid_recovery', 2, 2), ('covid_recovery', 3, 3), ('covid_recovery', 4, 4), ('covid_recovery', 5, 5), ('covid_recovery', 6, 6), ('covid_recovery', 7, 7), ('covid_recovery', 8, 8), ('covid_recovery', 9, 7), ('covid_recovery', 10, 5),
    ('covid_mild', 1, 1), ('covid_mild', 2, 1), ('covid_mild', 3, 2), ('covid_mild', 4, 3), ('covid_mild', 5, 4), ('covid_mild', 6, 5), ('covid_mild', 7, 6), ('covid_mild', 8, 6), ('covid_mild', 9, 5), ('covid_mild', 10, 4),
    ('covid_pagdaraoan', 1, 2), ('covid_pagdaraoan', 2, 3), ('covid_pagdaraoan', 3, 4), ('covid_pagdaraoan', 4, 5), ('covid_pagdaraoan', 5, 6), ('covid_pagdaraoan', 6, 8), ('covid_pagdaraoan', 7, 9), ('covid_pagdaraoan', 8, 8), ('covid_pagdaraoan', 9, 7), ('covid_pagdaraoan', 10, 5),
    ('lepto_hotspot', 1, 1), ('lepto_hotspot', 2, 2), ('lepto_hotspot', 3, 3), ('lepto_hotspot', 4, 5), ('lepto_hotspot', 5, 7), ('lepto_hotspot', 6, 9), ('lepto_hotspot', 7, 12), ('lepto_hotspot', 8, 15), ('lepto_hotspot', 9, 13), ('lepto_hotspot', 10, 10),
    ('lepto_medium', 1, 1), ('lepto_medium', 2, 2), ('lepto_medium', 3, 3), ('lepto_medium', 4, 4), ('lepto_medium', 5, 6), ('lepto_medium', 6, 8), ('lepto_medium', 7, 10), ('lepto_medium', 8, 11), ('lepto_medium', 9, 9), ('lepto_medium', 10, 7),
    ('lepto_mild', 1, 1), ('lepto_mild', 2, 1), ('lepto_mild', 3, 2), ('lepto_mild', 4, 2), ('lepto_mild', 5, 3), ('lepto_mild', 6, 4), ('lepto_mild', 7, 5), ('lepto_mild', 8, 4), ('lepto_mild', 9, 3), ('lepto_mild', 10, 2),
    ('biday_lepto', 1, 1), ('biday_lepto', 2, 1), ('biday_lepto', 3, 2), ('biday_lepto', 4, 2), ('biday_lepto', 5, 3), ('biday_lepto', 6, 4), ('biday_lepto', 7, 5), ('biday_lepto', 8, 5), ('biday_lepto', 9, 4), ('biday_lepto', 10, 3),
    ('tanqui_lepto', 1, 1), ('tanqui_lepto', 2, 1), ('tanqui_lepto', 3, 2), ('tanqui_lepto', 4, 3), ('tanqui_lepto', 5, 4), ('tanqui_lepto', 6, 5), ('tanqui_lepto', 7, 6), ('tanqui_lepto', 8, 6), ('tanqui_lepto', 9, 5), ('tanqui_lepto', 10, 3),
    ('lepto_camansi', 1, 1), ('lepto_camansi', 2, 2), ('lepto_camansi', 3, 2), ('lepto_camansi', 4, 3), ('lepto_camansi', 5, 4), ('lepto_camansi', 6, 5), ('lepto_camansi', 7, 7), ('lepto_camansi', 8, 6), ('lepto_camansi', 9, 5), ('lepto_camansi', 10, 4);

DROP TEMPORARY TABLE IF EXISTS seed_combo_plan;
CREATE TEMPORARY TABLE seed_combo_plan (
    Barangay VARCHAR(100) NOT NULL,
    Disease VARCHAR(100) NOT NULL,
    BhwArea VARCHAR(100) NOT NULL,
    ProfileName VARCHAR(64) NOT NULL,
    PRIMARY KEY (Barangay, Disease)
);

INSERT INTO seed_combo_plan (Barangay, Disease, BhwArea, ProfileName)
VALUES
    ('Sevilla', 'Dengue', 'Sevilla', 'dengue_hotspot'),
    ('Sevilla', 'Flu', 'Sevilla', 'flu_watch'),
    ('Sevilla', 'COVID-19', 'Sevilla', 'covid_watch'),
    ('Sevilla', 'Leptospirosis', 'Sevilla', 'lepto_mild'),

    ('Catbangen', 'Dengue', 'Catbangen', 'dengue_rise'),
    ('Catbangen', 'Flu', 'Catbangen', 'flu_hotspot'),
    ('Catbangen', 'COVID-19', 'Catbangen', 'covid_watch'),
    ('Catbangen', 'Leptospirosis', 'Catbangen', 'lepto_mild'),

    ('San Vicente', 'Dengue', 'San Vicente', 'dengue_medium'),
    ('San Vicente', 'Flu', 'San Vicente', 'flu_watch'),
    ('San Vicente', 'COVID-19', 'San Vicente', 'covid_hotspot'),
    ('San Vicente', 'Leptospirosis', 'San Vicente', 'lepto_medium'),

    ('Pagdalagan', 'Dengue', 'Sevilla', 'dengue_rise'),
    ('Pagdalagan', 'Flu', 'Sevilla', 'flu_recovery'),
    ('Pagdalagan', 'COVID-19', 'Sevilla', 'covid_recovery'),
    ('Pagdalagan', 'Leptospirosis', 'Sevilla', 'lepto_hotspot'),

    ('Biday', 'Dengue', 'Catbangen', 'watch_moderate'),
    ('Biday', 'Flu', 'Catbangen', 'flu_biday'),
    ('Biday', 'COVID-19', 'Catbangen', 'covid_mild'),
    ('Biday', 'Leptospirosis', 'Catbangen', 'biday_lepto'),

    ('Tanqui', 'Dengue', 'Sevilla', 'dengue_tanqui'),
    ('Tanqui', 'Flu', 'Sevilla', 'flu_medium'),
    ('Tanqui', 'COVID-19', 'Sevilla', 'covid_mild'),
    ('Tanqui', 'Leptospirosis', 'Sevilla', 'tanqui_lepto'),

    ('Pagdaraoan', 'Dengue', 'San Vicente', 'dengue_pagdaraoan'),
    ('Pagdaraoan', 'Flu', 'San Vicente', 'flu_recovery'),
    ('Pagdaraoan', 'COVID-19', 'San Vicente', 'covid_pagdaraoan'),
    ('Pagdaraoan', 'Leptospirosis', 'San Vicente', 'covid_mild'),

    ('Camansi', 'Dengue', 'Catbangen', 'watch_moderate'),
    ('Camansi', 'Flu', 'Catbangen', 'flu_camansi'),
    ('Camansi', 'COVID-19', 'Catbangen', 'covid_mild'),
    ('Camansi', 'Leptospirosis', 'Catbangen', 'lepto_camansi');

INSERT INTO health_records (PatientID, BHWID, DateRecorded, Disease, Symptoms, Status)
SELECT
    prepared.PatientID,
    prepared.BHWID,
    prepared.DateRecorded,
    prepared.Disease,
    prepared.Symptoms,
    prepared.Status
FROM (
    SELECT
        cp.Barangay,
        pp.PatientID,
        bhw.UserID AS BHWID,
        TIMESTAMP(
            DATE_ADD(sw.WeekStart, INTERVAL MOD(sn.n - 1, 7) DAY),
            MAKETIME(
                8 + MOD(
                    sw.WeekNo + sn.n +
                    CASE cp.Disease
                        WHEN 'Dengue' THEN 0
                        WHEN 'Flu' THEN 1
                        WHEN 'COVID-19' THEN 2
                        WHEN 'Leptospirosis' THEN 3
                        ELSE 0
                    END,
                    9
                ),
                CASE WHEN MOD(sn.n, 2) = 0 THEN 15 ELSE 40 END,
                0
            )
        ) AS DateRecorded,
        cp.Disease,
        CASE cp.Disease
            WHEN 'Dengue' THEN
                CASE MOD(sn.n - 1, 4)
                    WHEN 0 THEN 'High fever, severe headache, joint pain'
                    WHEN 1 THEN 'Fever, retro-orbital pain, muscle aches'
                    WHEN 2 THEN 'Persistent fever, rash, fatigue'
                    ELSE 'Body pain, headache, loss of appetite'
                END
            WHEN 'Flu' THEN
                CASE MOD(sn.n - 1, 4)
                    WHEN 0 THEN 'Fever, cough, sore throat'
                    WHEN 1 THEN 'Runny nose, body aches, headache'
                    WHEN 2 THEN 'Chills, nasal congestion, fatigue'
                    ELSE 'Dry cough, weakness, low-grade fever'
                END
            WHEN 'COVID-19' THEN
                CASE MOD(sn.n - 1, 4)
                    WHEN 0 THEN 'Fever, dry cough, fatigue'
                    WHEN 1 THEN 'Sore throat, fever, headache'
                    WHEN 2 THEN 'Loss of taste, cough, body aches'
                    ELSE 'Nasal congestion, weakness, mild shortness of breath'
                END
            WHEN 'Leptospirosis' THEN
                CASE MOD(sn.n - 1, 4)
                    WHEN 0 THEN 'Fever, severe muscle pain, headache'
                    WHEN 1 THEN 'Fever, calf tenderness, weakness'
                    WHEN 2 THEN 'Chills, myalgia, red eyes'
                    ELSE 'Body pain, nausea, persistent fever'
                END
        END AS Symptoms,
        CASE
            WHEN sw.WeekNo <= 3 THEN
                CASE
                    WHEN pc.CaseCount >= 12 AND sn.n >= pc.CaseCount - 1 THEN 'Critical'
                    ELSE 'Active'
                END
            WHEN sw.WeekNo <= 7 THEN
                CASE
                    WHEN pc.CaseCount >= 14 AND sn.n = pc.CaseCount THEN 'Critical'
                    WHEN MOD(sn.n, 2) = 0 THEN 'Recovered'
                    ELSE 'Active'
                END
            ELSE
                CASE
                    WHEN pc.CaseCount >= 15 AND sn.n = pc.CaseCount THEN 'Critical'
                    WHEN MOD(sn.n, 4) = 0 THEN 'Active'
                    ELSE 'Recovered'
                END
        END AS Status
    FROM seed_combo_plan cp
    INNER JOIN seed_profile_cases pc
        ON pc.ProfileName = cp.ProfileName
    INNER JOIN seed_weeks sw
        ON sw.WeekNo = pc.WeekNo
    INNER JOIN seed_numbers sn
        ON sn.n <= pc.CaseCount
    INNER JOIN users bhw
        ON bhw.Role = 'BHW'
       AND bhw.AssignedArea = cp.BhwArea
    INNER JOIN (
        SELECT
            TRIM(SUBSTRING_INDEX(h.Address, ',', 1)) AS Barangay,
            hm.PatientID,
            ROW_NUMBER() OVER (
                PARTITION BY TRIM(SUBSTRING_INDEX(h.Address, ',', 1))
                ORDER BY hm.IsEmergencyContact, hm.FullName, hm.PatientID
            ) AS PatientRank,
            COUNT(*) OVER (
                PARTITION BY TRIM(SUBSTRING_INDEX(h.Address, ',', 1))
            ) AS PatientCount
        FROM household_members hm
        INNER JOIN households h
            ON h.HouseholdID = hm.HouseholdID
    ) pp
        ON pp.Barangay = cp.Barangay
       AND pp.PatientRank = MOD(sw.WeekNo + sn.n - 2, pp.PatientCount) + 1
) AS prepared
WHERE NOT EXISTS (
    SELECT 1
    FROM health_records hr
    WHERE hr.PatientID = prepared.PatientID
      AND hr.BHWID = prepared.BHWID
      AND hr.DateRecorded = prepared.DateRecorded
      AND hr.Disease = prepared.Disease
      AND hr.Symptoms = prepared.Symptoms
      AND hr.Status = prepared.Status
)
ORDER BY prepared.DateRecorded, prepared.Barangay, prepared.Disease, prepared.PatientID;

DROP TEMPORARY TABLE IF EXISTS seed_combo_plan;
DROP TEMPORARY TABLE IF EXISTS seed_profile_cases;
DROP TEMPORARY TABLE IF EXISTS seed_weeks;
DROP TEMPORARY TABLE IF EXISTS seed_numbers;

COMMIT;
