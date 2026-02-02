# KQL Observability Queries

Dynamic KQL queries for monitoring PoAppIdea in Application Insights.

## 1. API Endpoint Latency (P50/P95/P99)

Track response times across all API endpoints.

```kql
// API Endpoint Latency Distribution
requests
| where timestamp > ago(24h)
| where name startswith "POST /api" or name startswith "GET /api"
| summarize 
    P50 = percentile(duration, 50),
    P95 = percentile(duration, 95),
    P99 = percentile(duration, 99),
    Count = count()
    by name
| order by P95 desc
| project 
    Endpoint = name,
    P50_ms = round(P50, 1),
    P95_ms = round(P95, 1),
    P99_ms = round(P99, 1),
    Requests = Count
```

### Alert Threshold
- **Warning**: P95 > 500ms
- **Critical**: P95 > 2000ms

---

## 2. AI Service Errors and Rate Limits

Monitor Azure OpenAI failures and rate limiting.

```kql
// AI Service Error Tracking
dependencies
| where timestamp > ago(24h)
| where target contains "openai" or name contains "ChatCompletion" or name contains "ImageGeneration"
| summarize 
    Total = count(),
    Failures = countif(success == false),
    AvgDuration = avg(duration)
    by bin(timestamp, 1h), name
| extend FailureRate = round(100.0 * Failures / Total, 2)
| project timestamp, 
    Operation = name, 
    Total, 
    Failures, 
    FailureRate_pct = FailureRate,
    AvgDuration_ms = round(AvgDuration, 0)
| order by timestamp desc
```

### Additional AI Metrics
```kql
// Rate Limit Detection (429 errors)
dependencies
| where timestamp > ago(1h)
| where target contains "openai"
| where resultCode == "429"
| summarize RateLimitHits = count() by bin(timestamp, 5m)
| render timechart
```

---

## 3. Session Completion Funnel

Track user progression through phases.

```kql
// Session Phase Funnel
customEvents
| where timestamp > ago(7d)
| where name in ("SessionCreated", "SparkCompleted", "MutationsCompleted", 
                 "FeaturesCompleted", "SynthesisCompleted", "RefinementCompleted", 
                 "ArtifactsGenerated")
| summarize Users = dcount(user_Id) by name
| order by case(
    name == "SessionCreated", 1,
    name == "SparkCompleted", 2,
    name == "MutationsCompleted", 3,
    name == "FeaturesCompleted", 4,
    name == "SynthesisCompleted", 5,
    name == "RefinementCompleted", 6,
    name == "ArtifactsGenerated", 7,
    99)
| project Phase = name, UniqueUsers = Users
```

### Conversion Rate Query
```kql
// Phase-to-Phase Conversion
let sessions = customEvents
| where timestamp > ago(7d)
| where name == "SessionCreated"
| summarize SessionsCreated = dcount(session_Id);
let completed = customEvents
| where timestamp > ago(7d)
| where name == "ArtifactsGenerated"
| summarize SessionsCompleted = dcount(session_Id);
sessions | join completed on $left.SessionsCreated == $left.SessionsCreated
| project 
    SessionsStarted = SessionsCreated,
    SessionsCompleted,
    ConversionRate = round(100.0 * SessionsCompleted / SessionsCreated, 2)
```

---

## 4. Rate Limiting and Throttling

Monitor rate limit middleware behavior.

```kql
// Rate Limiting Activity
traces
| where timestamp > ago(24h)
| where message contains "Rate limit" or message contains "RateLimiting"
| summarize 
    TotalEvents = count(),
    Blocked = countif(message contains "exceeded" or message contains "blocked"),
    Allowed = countif(message contains "allowed" or message contains "passed")
    by bin(timestamp, 15m)
| project timestamp, TotalEvents, Blocked, Allowed,
    BlockRate = round(100.0 * Blocked / TotalEvents, 2)
| render timechart
```

### Top Rate-Limited IPs
```kql
// Top Rate-Limited Clients
traces
| where timestamp > ago(1h)
| where message contains "Rate limit exceeded"
| extend ClientIP = extract("client: ([0-9.]+)", 1, message)
| summarize BlockCount = count() by ClientIP
| top 10 by BlockCount
```

---

## 5. Health Check Trends

Monitor health endpoint responses over time.

```kql
// Health Check Status Over Time
requests
| where timestamp > ago(24h)
| where name contains "/health"
| summarize 
    Total = count(),
    Healthy = countif(resultCode == "200"),
    Unhealthy = countif(resultCode != "200"),
    AvgDuration = avg(duration)
    by bin(timestamp, 5m)
| extend HealthRate = round(100.0 * Healthy / Total, 2)
| project timestamp, 
    HealthRate_pct = HealthRate, 
    AvgDuration_ms = round(AvgDuration, 0),
    Unhealthy
| render timechart
```

### Dependency Health Breakdown
```kql
// Health Check Component Status
customMetrics
| where timestamp > ago(1h)
| where name startswith "health_"
| summarize AvgValue = avg(value) by name, bin(timestamp, 1m)
| project timestamp, 
    Component = replace_string(name, "health_", ""),
    Status = iff(AvgValue == 1, "Healthy", "Unhealthy")
| render timechart
```

---

## Dashboard Setup

### Recommended Tiles

| Tile | Query | Visualization |
|------|-------|---------------|
| API Latency | Query 1 | Table |
| AI Errors (24h) | Query 2 | Time Chart |
| Session Funnel | Query 3 | Funnel Chart |
| Rate Limits | Query 4 | Time Chart |
| Health Trend | Query 5 | Time Chart |

### Alert Rules

| Alert | Condition | Severity |
|-------|-----------|----------|
| High API Latency | P95 > 2000ms for 5min | Warning |
| AI Failures | Failure rate > 5% | Critical |
| Low Completion | Conversion < 10% | Warning |
| Health Degraded | HealthRate < 99% | Critical |
