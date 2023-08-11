![TruQuest Logo](https://github.com/rho-cassiopeiae/TruQuest/blob/dev/src/frontend/assets/images/logo.png?raw=true)
## Promise/trustworthiness tracker powered by Ethereum.

Keep track of people and companies' promises.<br>
Do they fulfill them? Are they worthy of your trust?

People have short memories. Blockchain never forgets.

## Table of contents
- [Gallery](#gallery)
- [How it works](#how-it-works)
  - [Promise submission](#promise-submission)
  - [Settlement proposal submission](#settlement-proposal-submission)
- [Current status](#current-status)
- [Roadmap](#roadmap)
- [Beyond 1.0](#beyond-1-0)
- [Technology stack](#technology-stack)

<a name="gallery" />

## Gallery
<p float="left">
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/69e37787-63e1-4bbe-a39d-cf8dd502783a" width="40%" />
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/b3d8ab98-9294-435d-b688-84b82e8a04d0" width="40%" />
</p>
<p float="left">
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/4c4fa193-31ae-4c98-99c6-91916fad3e45" width="40%" />
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/2a4b3a63-7df4-424d-8d69-74a1fae68c7c" width="40%" />
</p>
<p float="left">
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/45c06b4a-654e-4c21-91b9-159809ca57d6" width="40%" />
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/09a942b1-8b95-45de-b069-b06fde7d61e0" width="40%" />
</p>
<p float="left">
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/aaff30f4-fe7c-4fb1-8c85-721942c7ed3c" width="40%" />
    <img src="https://github.com/rho-cassiopeiae/TruQuest/assets/84779039/90cd16ee-392b-4f4b-9356-03d95e6629eb" width="40%" />
</p>

<a name="how-it-works" />

## How it works

<a name="promise-submission" />

### Promise submission
- User creates a subject of interest (a person or an organization) if it's not already present on the platform.
- User submits a promise that the subject allegedly made, providing proof (e.g. interview articles, tweets, etc.) confirming that the promise is valid.
- User funds the promise with Truthserum (ERC-20 token used by the platform), which kick-starts an evaluation process.
- Anyone other than the promise submitter can join a verifier selection lottery. The purpose of the lottery is to select N verifiers that would evaluate the promise's validity (this evaluation is only concerned with the promise's *validity* (i.e. that the subject has really made it), not with the promise's *fulfillment* (that comes later)).
- All lottery participants have to stake some Truthserum in order to join to make the process more resistant to Sybil attacks. Upon lottery completion every participant apart from the winners gets the staked funds back, while the winners proceed to the next phase: poll.
- The selected verifiers study the submission, check out the proof, etc., and, once satisfied, they vote on whether to accept the promise as valid or not.
- If the promise is accepted, the submitter gets his funds back plus some reward. If it's rejected, he gets his funds back minus some potential penalty. There could be no penalty if the verifiers deemed it unnecessary. For example, a user deserves a penalty if he tries to deliberately sneak in an invalid promise. But maybe the promise is actually valid and it's just the provided proof that is not compelling enough. In this case the verifiers might vote to "soft decline" the submission specifying a reason, so that the promise is not accepted by the platform but it's submitter is also not penalized and can try again with more complete proof later.
- Verifiers are also subject to rewards/penalties. Currently, a very simple algorithm is used: verifiers get rewared if they voted in accordance with the majority, and penalized otherwise.

<a name="settlement-proposal-submission" />

### Promise settlement proposal submission
- A promise can have a rigid time frame, a vague one, or no time frame at all. So it's up to the platform users to decide when to settle promises.
- Some user decides that the promise from above can now be settled (i.e. there is enough evidence to conclusively state that the subject has either fulfilled it or not). He creates a promise settlement proposal, in which he outlines his verdict, provides details, proof, etc. and submits it for evaluation.
- Often you can't just answer yes or no to the question of promise fulfillment. There could be some grey areas like a subject fulfilling it but not in the specified time frame. It's also important to distinguish between promises that are made in a deliberate attempt to deceive from those that are made in good faith and not kept because of some respectable circumstances. For these and many other reasons there are currently 6 different verdicts covering the range from "Yes, 100%" to "No, it was never intended to be fulfilled in the first place".
- The submitter funds the proposal, which kick-starts an evaluation process, which proceeds through the same stages as the promise evaluation with some minor alterations.
- If the proposal gets accepted the promise is considered settled, and it now counts towards the subject's trustworthiness rating. For example, if the accepted verdict was that the subject didn't keep his word and the promise was a deliberate lie designed to attract voters or something, then he gets a heavy hit on his reputation. 

Over time more and more promises get submitted and subjects get their ratings adjusted accordingly, which would enable us not only to calculate their overall ratings but also to create promise "heat maps" to track trends, because sometimes current trajectory is more important than past deeds.

<a name="current-status" />

## Current status
MVP is 90% ready and will go live on OP Goerli/Base Goerli (still thinking which one) in September 2023.

<a name="roadmap" />

## Roadmap
- MVP live on OP Goerli/Base Goerli in September 2023. This is a closed pre-alpha stage. Main goals:
	- Decide on a particular token design and distribution strategy. Current implementation is suitable for testing purposes but not for production.
	- Use feedback to tune the reward/penalty algorithms.
	- Possibly make changes to the overall user–platform interaction flow. Might need to make certain actions more Web2-like in terms of UX.
- Transitioning to open alpha is predicated on the state of ERC-4337 (aka Account Abstraction) infrastructure. TruQuest already uses AA but currently a lot of its "killer" features are not yet implemented by AA providers like Alchemy. For example, the only smart account type that Alchemy currently supports is `SimpleAccount`, which doesn't offer features like social recovery, multi-sig, and others. It is expected that til the end of 2023 we will see many production-ready AA smart wallet implementations (e.g. Soul wallet, Safe wallet, etc.) with much richer functionality compared to `SimpleAccount`. AA providers also work on their Paymaster implementations to enable sponsored (aka "gasless") transactions, which we want to take advantage of, since it would drastically simplify user onboarding.
- Open alpha on OP Goerli/Base Goerli (presumably at the end of 2023). Main goals:
	- Test sponsored transactions. Figure out per user limits and the like.
	- Test smart wallet social recovery flow.
	- Test infrastructure under (hopefully) heavier, more real world-like load.
	- Integrate with Ethereum Attestation Service.
	- Finalize smart contracts. Code audit.
	- Set up DAO (MVP doesn't have one). The purpose of DAO would be to allow users to vote to adjust certain things about the platform like lottery and poll durations, importance multipliers, or more drastically – replacing the orchestrator\* (the entity that calls smart contracts to initialize and close lotteries and polls, reward and penalize users, etc.). Full extent of rights and responsibilities to be decided.
- Open beta on OP Mainnet/Base Mainnet. Main goals:
	- Currently I'm thinking about killing two birds with one stone: ask users to fill the platform with historic data (since launching the platform empty would be underwhelming) and use the opportunity to do the initial token distribution based on users' contributions. For now it's just an idea. More complete plan is pending until the token design is decided upon.
- 1.0 launch on OP Mainnet/Base Mainnet.

\* Orchestrator does a lot of important things like calculating poll results off-chain and submitting it on-chain, so it has a lot of power. It's basically a superuser, there is no way around it. But there **is** a way in which it could be kept in check:

 - [x] Everything the orchestrator does is 100% verifiable from publicly available data. In other words, the code is open-source, the computation inputs (like votes) are public, and the outputs are public (on-chain). Anyone can take the inputs, run them through the algorithms, and verify the outputs.
 - [ ] The orchestrator could be replaced if DAO members vote to do it.

<a name="beyond-1-0" />

## Beyond 1.0
- Support for video evidence/proof. Preferably hosted using PeerTube.
- Mobile application.
- NFTs both for subjects with excellent ratings and users with great contributions to the platform.
- Multi-chain.

<a name="technology-stack" />

## Technology stack
- Dapp
	- Ethereum. Optimism
		- ERC-4337 Account Abstraction
		- Ethereum Attestation Service
	- Solidity
- Backend
	- .NET 7
	- PostgreSQL
	- Kafka
	- Debezium
	- IPFS
- Frontend
	- Flutter Web
	- JavaScript
