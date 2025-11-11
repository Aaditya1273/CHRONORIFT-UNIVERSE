/// ChronoRift Universe - $CRU Token Module
/// Manages the $CRU token economy for the game
module chronorift::cru_token {
    use sui::coin::{Self, Coin, TreasuryCap};
    use sui::balance::{Self, Balance};
    use sui::tx_context::{Self, TxContext};
    use sui::transfer;
    use sui::object::{Self, UID};

    /// The CRU token type
    public struct CRU has drop {}

    /// Treasury capability for minting/burning CRU tokens
    public struct CRUTreasury has key {
        id: UID,
        treasury_cap: TreasuryCap<CRU>,
        total_minted: u64,
        total_burned: u64,
    }

    /// Player token balance tracker
    public struct PlayerBalance has key {
        id: UID,
        player: address,
        earned: u64,
        spent: u64,
        staked: u64,
    }

    /// Initialize the CRU token
    fun init(witness: CRU, ctx: &mut TxContext) {
        let (treasury_cap, metadata) = coin::create_currency(
            witness,
            9, // decimals
            b"CRU",
            b"ChronoRift Universe Token",
            b"Earn CRU by completing temporal puzzles in ChronoRift Universe",
            option::none(),
            ctx
        );

        // Share the metadata
        transfer::public_freeze_object(metadata);

        // Create treasury
        let treasury = CRUTreasury {
            id: object::new(ctx),
            treasury_cap,
            total_minted: 0,
            total_burned: 0,
        };

        transfer::share_object(treasury);
    }

    /// Mint CRU tokens as reward for completing levels
    public entry fun mint_reward(
        treasury: &mut CRUTreasury,
        amount: u64,
        recipient: address,
        ctx: &mut TxContext
    ) {
        let coins = coin::mint(&mut treasury.treasury_cap, amount, ctx);
        treasury.total_minted = treasury.total_minted + amount;
        transfer::public_transfer(coins, recipient);
    }

    /// Burn CRU tokens (deflationary mechanism)
    public entry fun burn_tokens(
        treasury: &mut CRUTreasury,
        coins: Coin<CRU>,
    ) {
        let amount = coin::value(&coins);
        coin::burn(&mut treasury.treasury_cap, coins);
        treasury.total_burned = treasury.total_burned + amount;
    }

    /// Get total minted tokens
    public fun get_total_minted(treasury: &CRUTreasury): u64 {
        treasury.total_minted
    }

    /// Get total burned tokens
    public fun get_total_burned(treasury: &CRUTreasury): u64 {
        treasury.total_burned
    }

    /// Get circulating supply
    public fun get_circulating_supply(treasury: &CRUTreasury): u64 {
        treasury.total_minted - treasury.total_burned
    }
}
