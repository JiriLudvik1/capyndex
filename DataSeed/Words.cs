namespace Capyndex.DataSeeding;

public static class Words
{
    public static readonly string[] CommonWords =
    [
        // Original common words
        "the", "and", "is", "in", "to", "with", "of", "for", "a", "on",
        "data", "index", "search", "document", "content", "text", "information",
        "system", "application", "user", "query", "result", "analysis", "database",

        // Additional common words - general language
        "at", "by", "from", "that", "this", "it", "as", "be", "are", "was",
        "were", "have", "has", "had", "can", "will", "would", "should", "could",
        "may", "might", "must", "an", "but", "or", "if", "because", "so", "when",
        "where", "how", "what", "who", "which", "why", "not", "than", "then", "their",
        "there", "these", "those", "them", "they", "you", "your", "our", "my", "his",
        "her", "its", "we", "us", "do", "does", "did", "done", "all", "any", "some",
        "many", "much", "more", "most", "other", "such", "no", "new", "old", "time",
        "like", "just", "now", "about", "over", "under", "between", "after", "before",

        // Additional common words - tech specific
        "file", "files", "folder", "upload", "download", "server", "client", "cloud",
        "code", "program", "software", "hardware", "network", "internet", "web", "site",
        "page", "api", "service", "app", "id", "key", "value", "field", "record", "row",
        "column", "table", "list", "array", "object", "class", "method", "function",
        "input", "output", "error", "warning", "message", "notification", "alert",
        "email", "password", "login", "logout", "account", "profile", "settings", "config",
        "version", "update", "install", "build", "deploy", "run", "execute", "test",
        "debug", "log", "cache", "memory", "storage", "disk", "drive", "backup", "restore",
        "save", "load", "read", "write", "create", "delete", "edit", "modify", "change",
        "get", "set", "put", "post", "request", "response", "header", "body", "status",

        // New common words - additional general
        "each", "every", "while", "during", "through", "without", "within", "across",
        "among", "against", "along", "around", "since", "until", "though", "although",
        "despite", "except", "unless", "whether", "whose", "whom", "myself", "yourself",
        "himself", "herself", "itself", "ourselves", "themselves", "each", "both", "few",
        "several", "enough", "little", "less", "least", "another", "same", "different",

        // New common words - business/project oriented
        "team", "project", "plan", "goal", "task", "schedule", "deadline", "meeting",
        "report", "review", "milestone", "progress", "priority", "resource", "budget",
        "cost", "time", "risk", "issue", "solution", "decision", "approval", "feedback",
        "customer", "client", "partner", "vendor", "stakeholder", "requirement", "scope"
    ];

    public static readonly string[] LessCommonWords =
    [
        // Original less common words
        "algorithm", "repository", "efficient", "structure", "optimize", "retrieve",
        "compute", "storage", "interface", "pipeline", "process", "semantic",
        "vector", "relevance", "ranking", "performance", "scalable", "distributed",

        // Additional less common words - technical
        "abstraction", "architecture", "asynchronous", "authentication", "authorization",
        "bandwidth", "benchmark", "biometric", "blockchain", "caching", "clustering",
        "compression", "concurrency", "containerization", "cryptography", "datacenter",
        "deduplication", "denormalization", "dependency", "deployment", "deserialization",
        "deterministic", "diagnostic", "encapsulation", "encryption", "endpoint",
        "enumerate", "escalation", "extensibility", "extraction", "failover", "fault-tolerant",
        "federated", "fragmentation", "framework", "generative", "granularity", "heuristic",
        "hierarchical", "hypervisor", "idempotent", "immutable", "implementation", "inference",
        "infrastructure", "instantiation", "interpolation", "interoperability", "introspection",
        "invocation", "isolation", "kubernetes", "latency", "lexical", "lifecycle", "linearizable",
        "locality", "logarithmic", "manifest", "marshalling", "metadata", "microservice",
        "middleware", "migration", "mocking", "monolithic", "multitenant", "namespace",
        "normalization", "observability", "orchestration", "parallelism", "parameter",
        "partitioning", "persistence", "polymorphic", "preprocessing", "provisioning",
        "quadratic", "quantization", "recursion", "redundancy", "refactoring", "regression",
        "replication", "resilience", "schema", "serialization", "serverless", "sharding",
        "synchronization", "taxonomy", "telemetry", "throughput", "transaction", "transformation",
        "traversal", "utilization", "validation", "vectorization", "virtualization", "webhook",

        // AI and ML specific
        "backpropagation", "bayesian", "classification", "clustering", "convolutional",
        "dimensionality", "embeddings", "ensemble", "epoch", "feature", "gradient",
        "hyperparameter", "inference", "keras", "learning", "lstm", "neural", "normalization",
        "overfitting", "pytorch", "quantization", "recurrent", "regression", "reinforcement",
        "supervised", "tensorflow", "transformer", "underfitting", "unsupervised",

        // New less common words - advanced technical
        "amortized", "anisotropic", "anomaly-detection", "antipattern", "arbitrage",
        "atomicity", "autocorrelation", "autoscaling", "bifurcation", "bitmask",
        "Byzantine-fault", "canonicalization", "cardinality", "chromatography", "coefficiency",
        "combinatorial", "contextualization", "contravariant", "counterfactual", "cross-entropy",
        "cyclometric", "deconvolution", "differentiable", "disambiguation", "discretization",
        "eigenvector", "empirical", "entropy", "equiprobable", "ergodic", "etcd", "eviction",
        "evolutionary", "exabyte", "extrapolation", "finalization", "formalization", "fronthaul",
        "fuzzy-logic", "geospatial", "grapheme", "homomorphic", "hyperconverged", "impedance",
        "indirection", "inductive", "isomorphic", "juxtaposition", "kernelization",

        // New domain-specific terms
        "biocomputing", "cryptoeconomics", "cyberphysical", "devopsecfinops", "geofencing",
        "genomics", "geostationary", "homologation", "hydroinformatics", "immunoinformatics",
        "interferometry", "magnetoencephalography", "metabolomics", "metamorphic",
        "nanofabrication", "neuroevolution", "ontological", "ophthalmological", "pharmacokinetic",
        "phenomenological", "photogrammetry", "proteomics", "psychoacoustics", "radiometric",
        "seismological", "spectroscopic", "stochastic", "stratification", "teleological",
        "thermodynamic"
    ];
}