const API_BASE_URL = "http://localhost:5250/api";

let currentUsername = "SuperTrader";

document.addEventListener("DOMContentLoaded", () => {

    fetchMarketData();
    fetchUserProfile();
    fetchTransactionHistory();

    setInterval(fetchUserProfile, 3000);

    const eventSource = new EventSource(`${API_BASE_URL}/stocks/stream`);

    eventSource.onmessage = (event) => {
        const stockEvent = JSON.parse(event.data);
        updateLiveTickerUI(stockEvent);
    }

    eventSource.onerror = (err) => {
        console.error("SSE Connection lost. Reconnecting automatically...", err);
    }

    document.getElementById("loadProfileBtn").addEventListener("click", switchUser);
    document.getElementById("buyBtn").addEventListener("click", () => submitTrade(true));
    document.getElementById("sellBtn").addEventListener("click", () => submitTrade(false));
});

async function fetchMarketData() {
    try {
        const response = await fetch(`${API_BASE_URL}/stocks`);
        if (!response.ok) return;
        const stocks = await response.json();

        const tableBody = document.getElementById("marketTableBody");
        const selectDrop = document.getElementById("tradeSymbol");
        
        const currentSelection = selectDrop.value;

        tableBody.innerHTML = "";
        selectDrop.innerHTML = "";

        stocks.forEach(stock => {
            const row = `
                <tr class="text-sm hover:bg-gray-700/30 transition">
                    <td class="py-3 font-semibold text-emerald-400">${stock.symbol}</td>
                    <td class="py-3 text-gray-300">${stock.name}</td>
                    <td class="py-3 text-right font-mono text-white">$${stock.currentPrice.toFixed(2)}</td>
                </tr>
            `;
            tableBody.insertAdjacentHTML("beforeend", row);

            const option = document.createElement("option");
            option.value = stock.symbol;
            option.textContent = `${stock.symbol} - $${stock.currentPrice.toFixed(2)}`;
            selectDrop.appendChild(option);
        });

        if (currentSelection) selectDrop.value = currentSelection;

    } catch (error) {
        console.error("Error connecting to market watch endpoints:", error);
    }
}

async function fetchUserProfile() {
    try {
        const response = await fetch(`${API_BASE_URL}/profile/${currentUsername}`);
        if (!response.ok) {
            if (response.status === 404) {
                document.getElementById("portfolioCash").textContent = "$0.00";
                document.getElementById("portfolioNetWorth").textContent = "$0.00";
                document.getElementById("portfolioTableBody").innerHTML = `<tr><td colspan="6" class="text-center py-4 text-gray-500">User profile row placeholder not found. Please register first.</td></tr>`;
            }
            return;
        }
        
        const profile = await response.json();

        document.getElementById("portfolioUser").textContent = profile.username;
        document.getElementById("portfolioCash").textContent = `$${profile.availableCash.toLocaleString(undefined, {minimumFractionDigits: 2})}`;
        document.getElementById("portfolioNetWorth").textContent = `$${profile.totalPortfolioValue.toLocaleString(undefined, {minimumFractionDigits: 2})}`;

        const tableBody = document.getElementById("portfolioTableBody");
        tableBody.innerHTML = "";

        if(profile.holdings.length === 0){
            tableBody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-gray-500">No active positions owned. Buy assets above!</td></tr>`;
            return;
        }

        profile.holdings.forEach(item => {
            const plClass = item.profitLoss >= 0 ? "text-emerald-400" : "text-rose-400";
            const plSign = item.profitLoss >= 0 ? "+" : "";

            const row = `
                <tr class="text-sm hover:bg-gray-700/30 transition border-b border-gray-700/30">
                    <td class="py-3 font-semibold text-gray-200">${item.symbol} <span class="text-xs text-gray-500 font-normal block">${item.companyName}</span></td>
                    <td class="py-3 font-mono">${item.quantity}</td>
                    <td class="py-3 font-mono text-gray-400">$${item.averageBuyPrice.toFixed(2)}</td>
                    <td class="py-3 font-mono text-gray-400">$${item.currentPrice.toFixed(2)}</td>
                    <td class="py-3 font-mono font-medium">$${item.totalValue.toFixed(2)}</td>
                    <td class="py-3 text-right font-mono font-semibold ${plClass}">${plSign}$${item.profitLoss.toFixed(2)}</td>
                </tr>
            `;
            tableBody.insertAdjacentHTML("beforeend", row);
        });

    } catch (error) {
        console.error("Error querying user profile sheets:", error);
    }
}

async function submitTrade(isBuy) {
    const symbol = document.getElementById("tradeSymbol").value;
    const quantity = parseInt(document.getElementById("tradeQuantity").value);
    const msgBox = document.getElementById("statusMessage");

    if (!symbol || isNaN(quantity) || quantity <= 0) {
        showStatus("Please specify valid trading context quantities.", false);
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL}/trades/execute`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                username: currentUsername,
                symbol: symbol,
                quantity: quantity,
                isBuy: isBuy
            })
        });

        const data = await response.json();

        if (response.ok) {
            showStatus(data.message, true);
            fetchUserProfile();
            fetchTransactionHistory();
        } else {
            showStatus(data.message || "Execution error encountered.", false);
        }

    } catch (error) {
        showStatus("Could not reach backend gateway services context paths.", false);
    }
}


function switchUser() {
    const inputVal = document.getElementById("usernameInput").value.trim();
    if(inputVal) {
        currentUsername = inputVal;
        fetchUserProfile();
        fetchTransactionHistory();
        showStatus(`Loaded view context details tracking for user context: ${currentUsername}`, true);
    }
}

function showStatus(text, isSuccess) {
    const msgBox = document.getElementById("statusMessage");
    msgBox.textContent = text;
    msgBox.className = `mt-4 p-3 rounded-lg text-sm font-medium ${isSuccess ? 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20' : 'bg-rose-500/10 text-rose-400 border border-rose-500/20'}`;
    
    setTimeout(() => { msgBox.className = "hidden"; }, 4000);
}


function updateLiveTickerUI(stockEvent) {
    const tableBody = document.getElementById("marketTableBody");
    const selectDrop = document.getElementById("tradeSymbol");

    const rows = tableBody.getElementsByTagName("tr");
    for (let row of rows) {
        const symbolCell = row.cells[0];
        if (symbolCell && symbolCell.textContent === stockEvent.Symbol) {
            const priceCell = row.cells[2];
            
            const oldPrice = parseFloat(priceCell.textContent.replace('$', ''));
            const newPrice = stockEvent.NewPrice;
            
            priceCell.textContent = `$${newPrice.toFixed(2)}`;
            
            if (newPrice > oldPrice) {
                priceCell.className = "py-3 text-right font-mono text-emerald-400 font-bold transition-all duration-300";
            } else if (newPrice < oldPrice) {
                priceCell.className = "py-3 text-right font-mono text-rose-400 font-bold transition-all duration-300";
            }
            
            setTimeout(() => {
                priceCell.className = "py-3 text-right font-mono text-white transition-all duration-300";
            }, 500);
            break;
        }
    }

    for (let option of selectDrop.options) {
        if (option.value === stockEvent.Symbol) {
            option.textContent = `${stockEvent.Symbol} - $${stockEvent.NewPrice.toFixed(2)}`;
            break;
        }
    }
}

async function fetchTransactionHistory() {
    try {
        const response = await fetch(`${API_BASE_URL}/trades/history/${currentUsername}`);
        if (!response.ok) return;
        
        const history = await response.json();
        const tableBody = document.getElementById("historyTableBody");
        tableBody.innerHTML = "";

        if (history.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-gray-500">No recorded history log items found.</td></tr>`;
            return;
        }

        history.forEach(item => {
            const isBuy = item.isBuy;
            const actionText = isBuy ? "BUY" : "SELL";
            const actionClass = isBuy ? "text-emerald-400 bg-emerald-500/10 border border-emerald-500/20" : "text-rose-400 bg-rose-500/10 border border-rose-500/20";
            
            // Format UTC string cleanly to user's local timezone view
            const formattedDate = new Date(item.executeAt).toLocaleString();

            const row = `
                <tr class="text-sm hover:bg-gray-700/20 transition">
                    <td class="py-3 font-mono text-gray-400">${formattedDate}</td>
                    <td class="py-3">
                        <span class="px-2 py-0.5 rounded text-xs font-semibold ${actionClass}">${actionText}</span>
                    </td>
                    <td class="py-3 font-semibold text-gray-200">${item.symbol}</td>
                    <td class="py-3 font-mono">${item.quantity}</td>
                    <td class="py-3 font-mono text-gray-400">$${item.pricePerShare.toFixed(2)}</td>
                    <td class="py-3 text-right font-mono font-medium text-white">$${item.totalAmount.toFixed(2)}</td>
                </tr>
            `;
            tableBody.insertAdjacentHTML("beforeend", row);
        });
    } catch (error) {
        console.error("Error querying transaction logs:", error);
    }
}