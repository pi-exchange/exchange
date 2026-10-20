#!/usr/bin/env bash
# Проверка работоспособности: health -> markets -> orderbook. Код возврата 0 — всё в порядке.
set -u
BASE="${BASE_URL:-http://localhost:${API_PORT:-8080}}"

fail() { echo "FAIL: $1" >&2; exit 1; }

echo "[1/3] GET $BASE/health"
for i in $(seq 1 30); do
  health="$(curl -fsS "$BASE/health" 2>/dev/null)" && break
  sleep 2
done
[ -n "${health:-}" ] || fail "сервис не ответил за 60 секунд"
echo "$health" | grep -q '"status":"ok"' || fail "health: $health"
echo "      $health"

echo "[2/3] GET $BASE/api/v1/markets"
markets="$(curl -fsS "$BASE/api/v1/markets")" || fail "markets недоступен"
count="$(echo "$markets" | grep -o '"symbol"' | wc -l | tr -d ' ')"
[ "$count" = "2" ] || fail "ожидалось 2 рынка, получено $count: $markets"
echo "      $markets"

echo "[3/3] GET $BASE/api/v1/markets/BTC/USDT/orderbook"
book="$(curl -fsS "$BASE/api/v1/markets/BTC/USDT/orderbook")" || fail "orderbook недоступен"
echo "$book" | grep -q '"bids":\[' && echo "$book" | grep -q '"asks":\[' || fail "в ответе нет списков bids/asks: $book"
echo "      $book"

echo "OK: проект запущен и отвечает"
