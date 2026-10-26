CREATE TABLE IF NOT EXISTS clientes (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    nome TEXT NOT NULL,
    cpf TEXT NOT NULL UNIQUE,
    data_nascimento TEXT NOT NULL,
    email TEXT
);

CREATE TABLE IF NOT EXISTS dividas (
    id INTEGER PRIMARY KEY AUTOINCREMENT,
    cliente_id INTEGER NOT NULL,
    valor NUMERIC NOT NULL CHECK (valor > 0),
    paga INTEGER NOT NULL DEFAULT 0 CHECK (paga IN (0, 1)),
    data_criacao TEXT NOT NULL,
    data_pagamento TEXT,
    FOREIGN KEY (cliente_id) REFERENCES clientes (id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_dividas_cliente_id
    ON dividas (cliente_id);

CREATE UNIQUE INDEX IF NOT EXISTS ix_dividas_uma_em_aberto
    ON dividas (cliente_id)
    WHERE paga = 0;
