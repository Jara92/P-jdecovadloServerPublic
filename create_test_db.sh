docker run -d \
    --name=pujcovadlo_test_db \
    --restart=always \
    -p 8032:5432 \
    -e POSTGRES_USER=postgres \
    -e POSTGRES_PASSWORD=password \
    -e POSTGRES_DB=pujcovadlo \
    -e LANG=cs_CZ.utf8 \
    -e POSTGRES_INITDB_ARGS="--locale-provider=icu --icu-locale=cs-CZ" \
    pujcovadlo/postgres