# embed_defects.py
import sqlite3
import numpy as np
from sentence_transformers import SentenceTransformer
import struct
import os

# === CONFIG ===
DB_PATH = 'app.db'
TABLE_NAME = 'Defects'
TEXT_COMBINE = lambda title, desc: f"{title or ''} [SEP] {desc or ''}".strip()
MODEL_NAME = 'all-MiniLM-L6-v2'  # 384-dim, fast, offline
BATCH_SIZE = 32
# ==============

print("Loading model (first run downloads ~90MB)...")
model = SentenceTransformer(MODEL_NAME)

conn = sqlite3.connect(DB_PATH)
cursor = conn.cursor()

# Get all defects without embedding
cursor.execute(f"""
    SELECT rowid, Title, Description 
    FROM {TABLE_NAME} 
    WHERE Embedding IS NULL
""")
rows = cursor.fetchall()

if not rows:
    print("All defects already have embeddings!")
    conn.close()
    exit()

print(f"Computing embeddings for {len(rows)} defects...")

for i in range(0, len(rows), BATCH_SIZE):
    batch = rows[i:i + BATCH_SIZE]
    texts = [TEXT_COMBINE(row[1], row[2]) for row in batch]
    ids = [row[0] for row in batch]

    # Generate embeddings
    embeddings = model.encode(texts, batch_size=BATCH_SIZE, show_progress_bar=False)

    # Save to DB
    for rowid, embedding in zip(ids, embeddings):
        # float32 → bytes
        embedding_blob = embedding.astype(np.float32).tobytes()
        cursor.execute(f"""
            UPDATE {TABLE_NAME} 
            SET Embedding = ? 
            WHERE rowid = ?
        """, (embedding_blob, rowid))

    conn.commit()
    print(f"  Progress: {min(i + BATCH_SIZE, len(rows))}/{len(rows)}")

print("All embeddings saved to Defects.Embedding!")
conn.close()