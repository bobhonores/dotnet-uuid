CREATE TABLE WeatherData
(
    id          UNIQUEIDENTIFIER NOT NULL,
    date        DATE           NOT NULL,
    temperature DECIMAL(10, 2) NOT NULL,
    summary     VARCHAR(255)   NULL,
    CONSTRAINT PK__WeatherData PRIMARY KEY (id)
);
