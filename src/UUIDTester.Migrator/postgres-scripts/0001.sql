create table if not exists weather_data
(
    id          uuid not null,
    date        date not null,
    temperature decimal(10, 2) not null,
    summary     varchar(255)   null,
    primary key (id)
);
